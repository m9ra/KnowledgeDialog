using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    public class DataContainer
    {
        internal readonly static string UserInputVar = "$user_input";

        internal readonly static string NativeActionPrefix = "#";

        internal readonly static string NativeEvaluatorPrefix = "$";

        private string[] _currentPattern;

        private readonly List<SemanticItem> _data = new List<SemanticItem>();

        private readonly HashSet<string> _span = new HashSet<string>();

        private readonly HashSet<string> _phraseIndex = new HashSet<string>();

        private readonly Dictionary<string, NativeEvaluator> _evaluators = new Dictionary<string, NativeEvaluator>();

        private readonly Dictionary<string, NativeAction> _nativeActions = new Dictionary<string, NativeAction>();

        public IEnumerable<SemanticItem> GetData()
        {
            return _data;
        }

        public bool IsInSpan(string answer)
        {
            return _span.Contains(answer);
        }

        internal void AddSpanElement(string spanAnswer)
        {
            _span.Add(spanAnswer);
        }

        internal void Add(SemanticItem item)
        {
            foreach (var phrase in item.Phrases)
            {
                _phraseIndex.Add(phrase);
            }
            _data.Add(item);
        }

        internal DataContainer Add(string[] patterns, string question, string answer)
        {
            foreach (var pattern in patterns)
            {
                Add(SemanticItem.From(question, answer, Constraints.WithInput(pattern)));
            }
            return this;
        }

        internal void AddEvaluator(string evaluatorId, NativeEvaluator evaluator)
        {
            _evaluators.Add(evaluatorId, evaluator);
            AddSpanElement(evaluatorId);
        }

        internal void AddEvaluator(string evaluatorName, string question, NativeEvaluator evaluator)
        {
            var evaluatorId = NativeEvaluatorPrefix + $"{evaluatorName}-" + question.ToLower().Replace(' ', '_').Replace("?", "");
            Add(_currentPattern, question, evaluatorId);

            AddEvaluator(evaluatorId, evaluator);
        }

        internal void AddNativeAction(string nativeActionId, NativeAction action)
        {
            _nativeActions.Add(nativeActionId, action);
            AddSpanElement(nativeActionId);
        }

        internal NativeEvaluator GetEvalutor(string evalutorName)
        {
            _evaluators.TryGetValue(evalutorName, out var evalutor);
            return evalutor;
        }

        internal NativeAction GetNativeAction(string nativeActionName)
        {
            _nativeActions.TryGetValue(nativeActionName, out var evalutor);
            return evalutor;
        }

        public DataContainer Pattern(params string[] pattern)
        {
            _currentPattern = pattern;
            return this;
        }

        public DataContainer HowToDo(string description)
        {
            return Add(_currentPattern, Question.HowToDo, description);
        }

        public DataContainer HowToConvertToNumber(string description)
        {
            return Add(_currentPattern, Question.HowToConvertItToNumber, description);
        }


        public DataContainer HowToEvaluate(string description)
        {
            return Add(_currentPattern, Question.HowToEvaluate, description);
        }

        public DataContainer HowToDo(string evaluatorName, NativeEvaluator evaluator)
        {
            var evaluatorId = NativeEvaluatorPrefix + $"{evaluatorName}-how_to_do";
            HowToDo(evaluatorId);

            AddEvaluator(evaluatorId, evaluator);

            return this;
        }

        public DataContainer HowToDo(string actionName, NativeAction action)
        {
            var evaluatorId = NativeActionPrefix + actionName;
            HowToDo(evaluatorId);

            _nativeActions.Add(evaluatorId, action);
            AddSpanElement(evaluatorId);

            return this;
        }

        public DataContainer HowToEvaluate(string evaluatorName, NativeEvaluator evaluator)
        {
            var evaluatorId = NativeEvaluatorPrefix + $"{evaluatorName}-how_to_evaluate";
            HowToEvaluate(evaluatorId);

            AddEvaluator(evaluatorId, evaluator);

            return this;
        }

        public DataContainer IsTrue(string description)
        {
            return Add(_currentPattern, Question.IsItTrue, description);
        }

        public DataContainer HowToSimplify(string description)
        {
            return Add(_currentPattern, Question.HowToSimplify, description);
        }

        public DataContainer IsTrue(string evaluatorName, NativeEvaluator evaluator)
        {
            var evaluatorId = NativeEvaluatorPrefix + $"{evaluatorName}-is_true";
            IsTrue(evaluatorId);

            AddEvaluator(evaluatorId, evaluator);

            return this;
        }

        internal ParameterEvaluator ParamQuery(string question, string forcedParameterName = null)
        {
            return (context, parameterName) =>
            {
                var realParamterName = forcedParameterName ?? parameterName;
                return context.Query(realParamterName, question).FirstOrDefault();
            };
        }

        internal NativeEvaluator EvaluateCallArgsSpan(string actionName, NativeAction action, params string[] parameters)
        {
            var evaluators = new List<ParameterEvaluator>();
            foreach (var parameter in parameters)
            {
                ParameterEvaluator evaluator = (e, p) => e.EvaluateOne(p);
                evaluators.Add(evaluator);
            }

            return EvaluateCallArgs(actionName, action, parameters, evaluators);
        }

        internal NativeEvaluator EvaluateCallArgs(string actionName, NativeAction action, IEnumerable<string> parameters, IEnumerable<ParameterEvaluator> evaluators)
        {
            var actionId = NativeActionPrefix + actionName;
            _nativeActions.Add(actionId, action);

            AddSpanElement(actionId);

            return e =>
            {
                var evaluatedConstraints = new Constraints();
                foreach (var parameter in parameters.Zip(evaluators, Tuple.Create))
                {
                    var evaluatedParameter = parameter.Item2(e, parameter.Item1);
                    if (evaluatedParameter == null)
                        return null;

                    evaluatedConstraints = evaluatedConstraints.AddValue(parameter.Item1, evaluatedParameter);
                }

                return SemanticItem.From(e.Item.Question, actionId, evaluatedConstraints);
            };
        }

        internal DataContainer AddPatternFact(string question, string answer)
        {
            return Add(_currentPattern, question, answer);
        }
    }
}
