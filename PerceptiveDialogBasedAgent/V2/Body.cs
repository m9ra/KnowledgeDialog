using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    internal delegate bool NativeAction(SemanticItem input);

    internal delegate SemanticItem ParameterEvaluator(EvaluationContext context, string parameterName);

    class Body
    {
        internal readonly static string WhatShouldAgentDoNowQ = "what should agent do now ?";

        internal readonly static string HowToConvertToNumberQ = "how to convert $@ to number ?";

        internal readonly static string HowToDoQ = "how to do $@ ?";

        internal readonly static string IsItTrueQ = "is $@ true ?";

        internal readonly static string HowToEvaluateQ = "what does $@ mean ?";

        internal readonly static string UserInputVar = "$user_input";

        internal readonly static string NativeActionPrefix = "#";

        internal readonly static string NativeEvaluatorPrefix = "$";

        internal readonly EvaluatedDatabase Db = new EvaluatedDatabase();

        private readonly List<string> _outputCandidates = new List<string>();

        private readonly List<string> _inputHistory = new List<string>();

        private readonly Stack<string> _scopes = new Stack<string>();

        private readonly Dictionary<string, SemanticItem> _slots = new Dictionary<string, SemanticItem>();

        private readonly Dictionary<string, DatabaseHandler> _databases = new Dictionary<string, DatabaseHandler>();

        private readonly Dictionary<string, NativeAction> _nativeActions = new Dictionary<string, NativeAction>();

        private readonly Dictionary<string, string> _outputReplacements = new Dictionary<string, string>();

        private readonly Queue<string> _eventQueue = new Queue<string>();

        private string _currentPattern;

        private static readonly List<Sensor> _sensors = new List<Sensor>();

        private readonly Sensor _dialogSensor = createSensor("dialog");

        private readonly Sensor _policySensor = createSensor("policy");

        private readonly Sensor _missingOutputSensor = createSensor("output is missing");

        private readonly Sensor _turnSensor = createSensor("turn");

        private readonly Sensor _inputProcessingSensor = createSensor("input processing");

        private readonly Sensor _outputPrintingSensor = createSensor("output printing");

        private readonly List<EvaluationLogEntry> _evaluationHistory = new List<EvaluationLogEntry>();

        internal IEnumerable<string> InputHistory => _inputHistory;

        internal ParameterEvaluator EvaluateOne
        {
            get
            {
                return (context, parameterName) =>
                {
                    return context.EvaluateOne(parameterName);
                };
            }
        }

        internal ParameterEvaluator Identity
        {
            get
            {
                return (context, parameterName) =>
                {
                    return context.GetSubstitution(parameterName);
                };
            }
        }

        internal Body()
        {
            this
                .Pattern("print $something")
                    .HowToDo("Print", EvaluateCallArgs("Print", _print, "$something"))

                .Pattern("$something is a command")
                    .IsTrue("$something has how to do question specified")

                .Pattern("the $something")
                    .HowToEvaluate("The", _the)

                .Pattern("one")
                    .HowToConvertToNumber("1")

                .Pattern("user input")
                    .HowToEvaluate("UserInput", _userInput)

                .Pattern("database question")
                    .HowToEvaluate("DatabaseQuestion", _databaseQuestion)

                .Pattern("history contains $something")
                    .IsTrue("HistoryContains", _historyContains)
                    
                .Pattern("$database database has $count result")
                    .IsTrue("ResultCount", _resultCountCondition)

                .Pattern("write $value into $slot slot")
                    .HowToDo("WriteSlot", EvaluateCallArgs("WriteSlot", _writeSlot, "$value", "$slot"))

                .Pattern("use $replacement instead of $pattern in output")
                    .HowToDo("OutputChanger", _outputChanger)

                .Pattern("set $database specifier $specifier")
                    .HowToDo("SetSpecifier",
                            EvaluateCallArgs("SetSpecifier", _setSpecifier, new[] { "$database", "$specifier", "$specifierClass" }, new[] { EvaluateOne, Identity, ParamQuery(RestaurantExtensions.WhatItSpecifiesQ, "$specifier") })
                        )

                .Pattern("value of $slot from $database database")
                    .HowToEvaluate("SlotValue", _slotValue)

                .Pattern("execute $something")
                    .HowToDo("Execute", e =>
                    {
                        var something = e.EvaluateOne("$something");
                        return something;
                    })

                .Pattern("~ $something")
                    .IsTrue("Not", _notCond)

                .Pattern("$value1 or $value2")
                    .IsTrue("Or", _orCond)

                .Pattern("$action1 and $action2")
                    .HowToDo("And", _and)

                .Pattern("$value1 and $value2")
                    .IsTrue("And", _andCond)

                .Pattern("$something has $question question specified")
                    .IsTrue("IsQuestionSpecified", e =>
                    {
                        var something = e.EvaluateOne("$something");
                        var question = e.GetSubstitutionValue("$question");
                        var normalizedQuestion = question + " $@ ?";
                        var queryItem = SemanticItem.AnswerQuery(normalizedQuestion, Constraints.WithInput(something.Answer));
                        var result = Db.Query(queryItem);
                        var answer = result.Any() ? Database.YesAnswer : Database.NoAnswer;

                        return SemanticItem.Entity(answer);
                    })

                .Pattern("add $action to trigger $trigger")
                    .HowToDo("TriggerAdd", _triggerAdd)

            ;
        }

        public string Input(string utterance)
        {
            Log.DialogUtterance("U: " + utterance);
            Db.StartQueryLog();

            _outputCandidates.Clear();

            //handle input processing
            _inputHistory.Add(utterance);

            if (_inputHistory.Count == 1)
                FireEvent("dialog started");

            FireEvent("user input is received");
            HandleAllEvents();

            finishInputProcessing();
            HandleAllEvents();

            //handle output processing
            var output = _outputCandidates.LastOrDefault();

            var log = Db.FinishLog();
            Log.Questions(log.GetQuestions());

            Log.DialogUtterance("S: " + output);
            return output;
        }

        public void PolicyInput(string utterance)
        {
            Log.Policy(utterance);

            Db.StartQueryLog();
            _outputCandidates.Clear();

            //handle input processing
            _inputHistory.Add(utterance);
            executeCommand(utterance);

            var log = Db.FinishLog();
            var questions = log.GetQuestions();
            Log.Questions(questions);

            // policy wont keep any history
            _inputHistory.Clear();
            _outputCandidates.Clear();
        }

        #region Event handling 

        internal void FireEvent(string eventDescription)
        {
            _eventQueue.Enqueue(eventDescription);
        }

        internal void HandleEvent(string eventDescription)
        {
            Log.EventHandler(eventDescription);
            var commands = Db.Query(SemanticItem.AnswerQuery(WhatShouldAgentDoNowQ, Constraints.WithCondition(eventDescription))).ToArray();
            foreach (var command in commands)
            {
                var result = executeCommand(command.Answer);
                /*if (!result)
                    throw new NotImplementedException("Handle failed command");*/
            }
        }

        internal void HandleAllEvents()
        {
            foreach (var database in _databases)
            {
                if (database.Value.IsUpdated)
                    FireEvent(database.Key + " database was updated");

                database.Value.IsUpdated = false;
            }

            if (_eventQueue.Count == 0)
                return;

            while (_eventQueue.Count > 0)
            {
                var eventDescription = _eventQueue.Dequeue();
                HandleEvent(eventDescription);
            }

            HandleAllEvents();
        }

        #endregion  

        internal Body AddDatabase(string databaseName, DatabaseHandler database)
        {
            _databases.Add(databaseName, database);
            return this;
        }

        public Body Pattern(string pattern)
        {
            _currentPattern = pattern;
            return this;
        }

        public Body HowToDo(string description)
        {
            return dbAdd(_currentPattern, HowToDoQ, description);
        }

        public Body HowToConvertToNumber(string description)
        {
            return dbAdd(_currentPattern, HowToConvertToNumberQ, description);
        }


        public Body HowToEvaluate(string description)
        {
            return dbAdd(_currentPattern, HowToEvaluateQ, description);
        }

        public Body HowToDo(string evaluatorName, NativeEvaluator evaluator)
        {
            var evaluatorId = NativeEvaluatorPrefix + $"{evaluatorName}-how_to_do";
            HowToDo(evaluatorId);

            Db.AddEvaluator(evaluatorId, evaluator);

            return this;
        }

        public Body HowToDo(string actionName, NativeAction action)
        {
            var evaluatorId = NativeActionPrefix + actionName;
            HowToDo(evaluatorId);

            _nativeActions.Add(evaluatorId, action);
            Db.AddSpanElement(evaluatorId);

            return this;
        }

        public Body HowToEvaluate(string evaluatorName, NativeEvaluator evaluator)
        {
            var evaluatorId = NativeEvaluatorPrefix + $"{evaluatorName}-how_to_evaluate";
            HowToEvaluate(evaluatorId);

            Db.AddEvaluator(evaluatorId, evaluator);

            return this;
        }

        public Body IsTrue(string description)
        {
            Db.Add(SemanticItem.Pattern(_currentPattern, Database.IsItTrueQ, description));
            return this;
        }

        public Body IsTrue(string evaluatorName, NativeEvaluator evaluator)
        {
            var evaluatorId = NativeEvaluatorPrefix + $"{evaluatorName}-is_true";
            IsTrue(evaluatorId);

            Db.AddEvaluator(evaluatorId, evaluator);

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

        internal NativeEvaluator EvaluateCallArgs(string actionName, NativeAction action, params string[] parameters)
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

            Db.AddSpanElement(actionId);

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

        internal Body AddPatternFact(string question, string answer)
        {
            dbAdd(_currentPattern, question, answer);
            return this;
        }

        private void finishInputProcessing()
        {
            if (_outputCandidates.Count == 0)
                FireEvent("output is missing");
        }


        private Body dbAdd(string pattern, string question, string answer)
        {
            Db.Add(SemanticItem.Pattern(pattern, question, answer));
            return this;
        }

        private static Sensor createSensor(string name)
        {
            var sensor = new Sensor(name);

            _sensors.Add(sensor);
            return sensor;
        }

        private bool executeCommand(string utterance)
        {
            return executeCommand(SemanticItem.Entity(utterance));
        }

        private bool executeCommand(SemanticItem command)
        {
            var commandQuery = SemanticItem.AnswerQuery(Body.HowToDoQ, Constraints.WithInput(command));
            var commandInterpretations = Db.SpanQuery(commandQuery).ToArray();

            foreach (var interpretation in commandInterpretations)
            {
                if (executeCall(interpretation))
                {
                    //we found a way how to execute the command
                    return true;
                }
            }

            return false;
        }

        private bool executeCall(SemanticItem command)
        {
            if (!_nativeActions.ContainsKey(command.Answer))
                return false;

            var action = _nativeActions[command.Answer];
            return action(command);
        }

        private IEnumerable<SemanticItem> getAnswers(string question)
        {
            var currentConstraints = createConstraintValues();
            var queryItem = SemanticItem.AnswerQuery(question, currentConstraints);

            var result = Db.Query(queryItem).ToArray();
            return result;
        }

        private IEnumerable<SemanticItem> getInputAnswers(string input, string question)
        {
            var currentConstraints = createConstraintValues();
            currentConstraints = currentConstraints.AddInput(input);
            var queryItem = SemanticItem.AnswerQuery(question, currentConstraints);

            var result = Db.Query(queryItem).ToArray();
            return result;
        }

        private Constraints createConstraintValues()
        {
            var constraints = new Constraints();
            foreach (var sensor in _sensors)
            {
                constraints = sensor.FillContext(constraints);
            }

            return constraints
                .AddInput(_inputHistory[_inputHistory.Count - 1])
            ;
        }

        private SemanticItem _notCond(EvaluationContext context)
        {
            var result = context.IsTrue("$something");

            throw new NotImplementedException();
        }

        private SemanticItem _orCond(EvaluationContext context)
        {
            var value1 = context.IsTrue("$value1");
            var value2 = context.IsTrue("$value2");

            throw new NotImplementedException();
        }

        private SemanticItem _andCond(EvaluationContext context)
        {
            var value1 = context.IsTrue("$value1");
            if (!value1)
                return SemanticItem.No;

            var value2 = context.IsTrue("$value2");

            return value2 ? SemanticItem.Yes : SemanticItem.No;
        }

        private bool _and(SemanticItem item)
        {
            var action1 = item.GetSubstitutionValue("$action1");
            var action2 = item.GetSubstitutionValue("$action2");

            var action1Result = executeCommand(action1);
            var action2Result = action1Result && executeCommand(action2);
            return action1Result && action2Result;
        }

        private bool _writeSlot(SemanticItem item)
        {
            var slot = item.GetSubstitutionValue("$slot");
            var value = item.GetSubstitution("$value");

            _slots[slot] = value;

            return true;
        }


        private SemanticItem _the(EvaluationContext item)
        {
            var evaluations = Db.EvaluationHistory.Reverse().Take(10).ToArray();
            var something = item.GetSubstitutionValue("$something");

            //find evaluation in context
            SemanticItem referencedValue = null;
            foreach (var evaluation in evaluations)
            {
                if (!evaluation.Result.Any())
                    continue;

                if (evaluation.Question != HowToEvaluateQ)
                    continue;

                if (evaluation.Input.Contains(something))
                {
                    referencedValue = evaluation.Result.First();
                }
            }

            return referencedValue;
        }

        private SemanticItem _databaseQuestion(EvaluationContext context)
        {
            var questions = Db.CurrentLogRoot.GetQuestions();
            var orderedQuestions = questions.OrderByDescending(rankQuestion).ToArray();
            var question = orderedQuestions.First();
            var questionStr = question.ReadableRepresentation();
            return SemanticItem.Entity(questionStr);
        }

        private double rankQuestion(SemanticItem question)
        {
            var q = question.Question;
            if (q == IsItTrueQ)
                return 0.1;

            if (q == SemanticItem.EntityQ)
                return 0.0;

            if (q == Body.HowToEvaluateQ)
                return 0.1;

            if (q == Body.WhatShouldAgentDoNowQ)
                return 0.1;

            return 1.0;
        }

        private SemanticItem _userInput(EvaluationContext context)
        {
            return SemanticItem.Entity(_inputHistory.Last());
        }

        private SemanticItem _historyContains(EvaluationContext context)
        {
            var something = context.GetSubstitutionValue("$something");
            throw new NotImplementedException();
        }


        private SemanticItem _resultCountCondition(EvaluationContext context)
        {
            var numberValue = context.Query("$count", HowToConvertToNumberQ).FirstOrDefault()?.Answer;
            if (numberValue == null)
                return null;

            if (!int.TryParse(numberValue, out var number))
                return null;

            var database = context.GetSubstitutionValue("$database");
            var count = _databases[database].ResultCount;

            return count == number ? SemanticItem.Yes : SemanticItem.No;
        }

        private SemanticItem _slotValue(EvaluationContext context)
        {
            var database = context.GetSubstitutionValue("$database");
            var slot = context.GetSubstitutionValue("$slot");

            return SemanticItem.Entity(_databases[database].Read(slot));
        }

        private bool _print(SemanticItem item)
        {
            var something = item.GetSubstitutionValue("$something");
            print(something);

            return true;
        }

        private bool _triggerAdd(SemanticItem item)
        {
            var action = item.GetSubstitutionValue("$action");
            var trigger = item.GetSubstitutionValue("$trigger");

            var constraints = new Constraints()
                .AddCondition(trigger);

            var actionItem = SemanticItem.From(WhatShouldAgentDoNowQ, action, constraints);
            Db.Add(actionItem);

            Log.SensorAdd(trigger, action);
            return true;
        }

        private bool _outputChanger(SemanticItem item)
        {
            var pattern = item.GetSubstitutionValue("$pattern");
            var replacement = item.GetSubstitutionValue("$replacement");

            _outputReplacements.Add(pattern, replacement);

            print("ok");
            return true;
        }

        private bool _setSpecifier(SemanticItem call)
        {
            var database = call.GetSubstitutionValue("$database");

            var specifierValue = call.GetSubstitutionValue("$specifier");
            var specifierCategory = call.GetSubstitution("$specifierClass").Answer;

            _databases[database].SetCriterion(specifierCategory, specifierValue);

            print($"{specifierValue} was set for {specifierCategory}");
            return true;
        }

        #region Body utilities

        private void print(string phrase)
        {
            var currentPhrase = " " + phrase + " ";
            foreach (var pattern in _outputReplacements)
            {
                currentPhrase = currentPhrase.Replace(pattern.Key, pattern.Value);
            }

            _outputCandidates.Add(currentPhrase.Trim());
        }

        #endregion
    }
}
