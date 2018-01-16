using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    delegate SemanticItem FieldEvaluator(ModuleContext context);

    delegate bool PreconditionEvaluator(ModuleContext context);

    abstract class BodyModuleBase
    {
        protected abstract void initializeAbilities();

        private readonly List<Tuple<ModuleField, string>> _parameters = new List<Tuple<ModuleField, string>>();

        private readonly Queue<string> _pendingEvents = new Queue<string>();

        private string _currentPattern = null;

        private string _currentQuestion = null;

        private PreconditionEvaluator _currentPrecondition = null;

        private bool _isInitialized = false;

        private MethodInfo _method;

        protected readonly DataContainer Container = new DataContainer();

        internal void Initialize()
        {
            if (_isInitialized)
                throw new NotSupportedException("Cannot initialize twice");

            initializeAbilities();
            processPattern();

            _isInitialized = true;
        }

        internal void AttachTo(EvaluatedDatabase database)
        {
            Initialize();

            database.RegisterContainer(Container);
        }

        internal IEnumerable<string> ReadEvents()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("module is not initialized");

            while (_pendingEvents.Count > 0)
                yield return _pendingEvents.Dequeue();
        }

        protected void FireEvent(string evt)
        {
            _pendingEvents.Enqueue(evt);
        }

        internal BodyModuleBase AddAbility(string pattern)
        {
            processPattern();

            _currentPattern = pattern;
            _currentQuestion = null;

            return this;
        }

        internal BodyModuleBase AddAnswer(string pattern, string question)
        {
            processPattern();

            _currentPattern = pattern;
            _currentQuestion = question;

            return this;
        }

        internal BodyModuleBase Precondition(PreconditionEvaluator evaluator)
        {
            _currentPrecondition = evaluator;
            return this;
        }

        internal BodyModuleBase CallAction(Action<string, string> call)
        {
            return Call(call.Method);
        }

        internal BodyModuleBase CallAction(Action call)
        {
            return Call(call.Method);
        }

        internal BodyModuleBase Call(Func<SemanticItem> call)
        {
            return Call(call.Method);
        }

        internal BodyModuleBase Call(Func<string, SemanticItem> call)
        {
            return Call(call.Method);
        }

        internal BodyModuleBase Call(Func<string, string, SemanticItem> call)
        {
            return Call(call.Method);
        }

        internal BodyModuleBase Call(Func<int, SemanticItem> call)
        {
            return Call(call.Method);
        }

        internal BodyModuleBase Call(MethodInfo method)
        {
            if (_method != null)
                throw new InvalidOperationException();

            _method = method;
            return this;
        }

        internal BodyModuleBase Param(ModuleField field, string source)
        {
            _parameters.Add(Tuple.Create(field, source));

            return this;
        }

        internal static ModuleField<T> newField<T>(string name, FieldEvaluator evaluator)
        {
            return new ModuleField<T>(name, evaluator);
        }

        private void processPattern()
        {
            if (_currentPattern == null)
            {
                if (_method != null)
                    throw new InvalidOperationException();

                if (_parameters.Count != 0)
                    throw new InvalidOperationException();

                return;
            }

            if (_currentQuestion != null)
            {
                processAnswerPattern();
            }
            else
            {
                processAbilityPattern();
            }

            _method = null;
            _currentQuestion = null;
            _currentPattern = null;
            _parameters.Clear();
        }

        private void processAnswerPattern()
        {
            var parametersCopy = _parameters.ToArray();
            var methodCopy = _method;
            var preconditionCopy = _currentPrecondition;
            var methodName = _method.Name;

            NativeEvaluator evaluator = c =>
            {
                var semanticArguments = new List<SemanticItem>();
                var context = new ModuleContext(c);
                foreach (var parameterPair in parametersCopy)
                {
                    var parameter = parameterPair.Item1;
                    var source = parameterPair.Item2;

                    var value = context.Evaluate(parameter, source);
                    semanticArguments.Add(value);
                }

                var prerequisitiesSatisfied = semanticArguments.All(a => a != null);
                if (!prerequisitiesSatisfied)
                    return null;

                if (preconditionCopy != null)
                {
                    var preconditionResult = preconditionCopy(context);
                    if (!preconditionResult)
                        return null;
                }

                var convertedParameters = getArguments(parametersCopy.Select(t => t.Item1).ToArray(), semanticArguments.ToArray());
                var evaluatorResult = methodCopy.Invoke(this, convertedParameters) as SemanticItem;

                return evaluatorResult;
            };

            Container.Pattern(_currentPattern)
                .AddEvaluator(methodName, _currentQuestion, evaluator);
        }

        private void processAbilityPattern()
        {
            var parametersCopy = _parameters.ToArray();
            var methodCopy = _method;
            var methodName = _method.Name;
            var nativeActionId = DataContainer.NativeActionPrefix + _method.Name;

            NativeEvaluator evaluator = c =>
            {
                var evaluatedValues = new List<Tuple<string, SemanticItem>>();
                var context = new ModuleContext(c);
                foreach (var parameterPair in parametersCopy)
                {
                    var parameter = parameterPair.Item1;
                    var source = parameterPair.Item2;

                    var value = context.Evaluate(parameter, source);
                    evaluatedValues.Add(Tuple.Create(parameter.Name, value));
                }

                var prerequisitiesSatisfied = evaluatedValues.All(t => t.Item2 != null);
                if (!prerequisitiesSatisfied)
                    return null;

                var call = encodeCall(nativeActionId, evaluatedValues);
                return call;
            };

            NativeAction action = c =>
            {
                var arguments = decodeArguments(parametersCopy.Select(t => t.Item1), c);
                methodCopy.Invoke(this, arguments);
                return true;
            };

            Container.Pattern(_currentPattern)
                .HowToDo(methodName, evaluator)
                .AddNativeAction(nativeActionId, action);
        }

        private object[] decodeArguments(IEnumerable<ModuleField> parameters, SemanticItem call)
        {
            var result = new List<object>();
            foreach (var parameter in parameters)
            {
                var semanticValue = call.Constraints.GetSubstitution(parameter.Name);
                var argument = parameter.GetInterpretation(semanticValue);
                result.Add(argument);
            }

            return result.ToArray();
        }

        private SemanticItem encodeCall(string nativeActionId, List<Tuple<string, SemanticItem>> evaluatedValues)
        {
            var arguments = new Constraints();
            foreach (var value in evaluatedValues)
            {
                arguments = arguments.AddValue(value.Item1, value.Item2);
            }

            return SemanticItem.Entity(nativeActionId).WithConstraints(arguments);
        }

        private object[] getArguments(ModuleField[] parameters, SemanticItem[] semanticArguments)
        {
            if (parameters.Length != semanticArguments.Length)
                throw new InvalidOperationException();

            var result = new List<object>();
            for (var i = 0; i < parameters.Length; ++i)
            {
                var value = parameters[i].GetInterpretation(semanticArguments[i]);
                result.Add(value);
            }

            return result.ToArray();
        }
    }

    class ModuleContext
    {
        internal readonly EvaluationContext EvaluationContext;

        private string _inputValue;

        private readonly Dictionary<ModuleField, object> _fields = new Dictionary<ModuleField, object>();

        internal string Input => _inputValue;

        internal ModuleContext(EvaluationContext evaluationContext)
        {
            EvaluationContext = evaluationContext;
        }

        public T Value<T>(ModuleField<T> index)
        {
            return (T)_fields[index];
        }

        internal SemanticItem GetAnswer(string question, string input)
        {
            var query = SemanticItem.AnswerQuery(question, Constraints.WithInput(input));

            return EvaluationContext.Query(query).LastOrDefault();
        }

        internal SemanticItem GetAnswer(string question)
        {
            return GetAnswer(question, _inputValue);
        }

        internal void SetInput(string input)
        {
            _inputValue = input;
        }

        internal SemanticItem Evaluate(ModuleField parameter, string source)
        {
            var lastInputValue = _inputValue;

            try
            {
                _inputValue = EvaluationContext.GetSubstitutionValue(source);
                var value = parameter.Evaluate(this);
                var interpretation = parameter.GetInterpretation(value);
                _fields[parameter] = interpretation;

                return value;
            }
            finally
            {
                _inputValue = lastInputValue;
            }
        }

        internal SemanticItem ChooseOption(string question, IEnumerable<string> columns)
        {
            if (columns.Contains(_inputValue))
                return SemanticItem.Entity(_inputValue);

            //TODO many strategies can be here
            var result = EvaluationContext.Query(SemanticItem.AnswerQuery(question, Constraints.WithInput(_inputValue)));
            return result.FirstOrDefault();
        }
    }

    class ModuleField<T> : ModuleField
    {
        private readonly FieldEvaluator _evaluator;

        internal override SemanticItem Evaluate(ModuleContext context)
        {
            return _evaluator(context);
        }

        internal override object GetInterpretation(SemanticItem item)
        {
            var value = item?.Answer;
            if (value == null)
                return null;

            var type = typeof(T);

            if (type == typeof(int))
            {
                int.TryParse(value, out var number);
                return number;
            }
            else
            {
                return value;
            }
        }

        internal ModuleField(string name, FieldEvaluator evaluator)
            : base(name)
        {
            _evaluator = evaluator;
        }
    }

    abstract class ModuleField
    {
        internal readonly string Name;

        internal abstract SemanticItem Evaluate(ModuleContext context);

        internal abstract object GetInterpretation(SemanticItem item);

        internal ModuleField(string name)
        {
            Name = name;
        }
    }
}
