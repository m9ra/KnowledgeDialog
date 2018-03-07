using PerceptiveDialogBasedAgent.V2.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    public delegate bool NativeAction(SemanticItem input);

    internal delegate SemanticItem ParameterEvaluator(EvaluationContext context, string parameterName);

    class Body
    {
        internal readonly EvaluatedDatabase Db = new EvaluatedDatabase();

        internal IEnumerable<SemanticItem> ExecutionQuestions => _executionQuestions;

        private readonly List<string> _outputCandidates = new List<string>();

        private readonly List<string> _inputHistory = new List<string>();

        private readonly List<BodyModuleBase> _modules = new List<BodyModuleBase>();

        private readonly Stack<string> _scopes = new Stack<string>();

        private readonly Dictionary<string, SemanticItem> _slots = new Dictionary<string, SemanticItem>();

        private readonly Dictionary<string, string> _outputReplacements = new Dictionary<string, string>();

        private readonly List<SemanticItem> _executionQuestions = new List<SemanticItem>();

        private readonly Queue<string> _eventQueue = new Queue<string>();

        private readonly HashSet<string> _firedEvents = new HashSet<string>();

        private readonly List<EvaluationLogEntry> _evaluationHistory = new List<EvaluationLogEntry>();

        internal IEnumerable<string> InputHistory => _inputHistory;

        private readonly CommandControlModule _commandControl;

        private readonly AdviceModule _adviceModule;

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
            Db.Container
                .Pattern("print $something")
                    .HowToDo("Print", Db.Container.EvaluateCallArgsSpan("Print", _print, "$something"))

                .Pattern("$something is a command")
                    .IsTrue("$something has how to do question specified")

                .Pattern("the $something")
                    .HowToEvaluate("The", _the)

                .Pattern("one")
                    .HowToConvertToNumber("1")

                .Pattern("user input")
                    .HowToEvaluate("UserInput", _userInput)

                .Pattern("history contains $something")
                    .IsTrue("HistoryContains", _historyContains)

                .Pattern("write $value into $slot slot")
                    .HowToDo("WriteSlot", Db.Container.EvaluateCallArgsSpan("WriteSlot", _writeSlot, "$value", "$slot"))

                .Pattern("$slot slot is filled")
                    .IsTrue("IsSlotFilled", _isSlotFilled)

                .Pattern("use $replacement instead of $pattern in output")
                    .HowToDo("OutputChanger", _outputChanger)

                .Pattern("$something can be an answer")
                    .IsTrue("CanBeAnswer", e =>
                    {
                        var result = e.Query("$something", Question.CanItBeAnswer).FirstOrDefault();

                        var canBeAnswer = result == null || result.Answer == Database.YesAnswer;
                        return canBeAnswer ? SemanticItem.Yes : SemanticItem.No;
                    })

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

                .Pattern("fire event $event")
                    .HowToDo("FireEvent", _fireEvent)

                .Pattern("$something joined with $something2")
                    .HowToEvaluate("JoinPhrases", _joinPhrases)

                .Pattern("dump database")
                    .HowToDo("DumpDatabase", c =>
                    {
                        Log.Dump(Db);
                        Print("ok");
                        return true;
                    })
            ;

            _commandControl = new CommandControlModule(this);
            _adviceModule = new AdviceModule(this);

            AddModule(_commandControl);
            AddModule(_adviceModule);
        }

        public string Input(string utterance)
        {
            var normalizedUtterance = normalizeInput(utterance);

            Log.DialogUtterance("U: " + normalizedUtterance);
            Db.StartQueryLog();

            _outputCandidates.Clear();

            //handle input processing
            _inputHistory.Add(normalizedUtterance);

            if (_inputHistory.Count == 1)
                FireEvent("dialog started");

            FireEvent("user input is received");
            HandleAllEvents();

            finishInputProcessing();
            HandleAllEvents();

            _executionQuestions.Clear();

            //handle output processing
            var output = _outputCandidates.LastOrDefault();

            var log = Db.FinishLog();
            Log.Questions(log.GetQuestions());

            var normalizedOutput = prettyPrintOutput(output);
            Log.DialogUtterance("S: " + normalizedOutput);
            return normalizedOutput;
        }

        public void PolicyInput(string utterance)
        {
            Log.Policy(utterance);

            Db.StartQueryLog();
            _outputCandidates.Clear();

            //handle input processing
            _inputHistory.Add(utterance);
            ExecuteCommand(utterance);

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
            if (!_firedEvents.Add(eventDescription))
                //event was already fired
                return;

            _eventQueue.Enqueue(eventDescription);
        }

        internal void HandleEvent(string eventDescription)
        {
            Log.EventHandler(eventDescription);
            var commands = Db.Query(SemanticItem.AnswerQuery(Question.WhatShouldAgentDoNow, Constraints.WithCondition(eventDescription))).ToArray();
            foreach (var command in commands)
            {
                var result = ExecuteCommand(command.Answer);
                /*if (!result)
                    throw new NotImplementedException("Handle failed command");*/
            }
        }

        internal void HandleAllEvents()
        {
            foreach (var module in _modules)
            {
                foreach (var moduleEvent in module.ReadEvents())
                    FireEvent(moduleEvent);
            }

            if (_eventQueue.Count == 0)
                //there are no events to process
                return;

            while (_eventQueue.Count > 0)
            {
                var eventDescription = _eventQueue.Dequeue();
                HandleEvent(eventDescription);
            }

            HandleAllEvents();

            _firedEvents.Clear();
        }

        #endregion  

        internal Body AddDatabase(string databaseName, DatabaseHandler database)
        {
            AddModule(new ExternalDatabaseProviderModule(databaseName, database));
            return this;
        }

        internal Body AddModule(BodyModuleBase module)
        {
            _modules.Add(module);
            module.AttachTo(Db);
            return this;
        }

        internal void ClearOutput()
        {
            _outputCandidates.Clear();
        }

        private string normalizeInput(string input)
        {
            var normalized = input.ToLowerInvariant().Replace("?", " ").Replace(".", " ").Replace(",", " ");

            string current;
            do
            {
                current = normalized;
                normalized = current.Replace("  ", " ");
            } while (current != normalized);

            return normalized.Trim();
        }

        private string prettyPrintOutput(string output)
        {
            if (output == null)
                return null;

            var o = " " + output + " ";
            o = o.Replace(" i ", " I ");
            o = o.Trim();
            o = char.ToUpper(o[0]) + o.Substring(1);

            if (!new[] { '.', '?', '!' }.Contains(o.Last()))
                return o + ".";

            o = o.Replace(" ?", "?").Replace(" .", ".").Replace(" !", "!");

            return o;
        }

        private void finishInputProcessing()
        {
            if (_outputCandidates.Count == 0)
            {
                _commandControl.ReportCurrentCommandFail();
                FireEvent("output is missing");
            }
        }

        internal bool ExecuteCommand(string utterance)
        {
            return executeCommand(SemanticItem.Entity(utterance));
        }

        private bool executeCommand(SemanticItem command)
        {
            var priorQuestions = Db.CurrentLogRoot.GetQuestions();

            var commandQuery = SemanticItem.AnswerQuery(Question.HowToDo, Constraints.WithInput(command));
            var commandInterpretations = Db.SpanQuery(commandQuery).ToArray();


            foreach (var interpretation in commandInterpretations)
            {
                if (executeCall(interpretation))
                {
                    //we found a way how to execute the command
                    return true;
                }
            }

            var postQuestions = Db.CurrentLogRoot.GetQuestions();
            var executionQuestions = postQuestions.Except(priorQuestions).ToArray();
            _executionQuestions.AddRange(executionQuestions);

            return false;
        }

        private bool executeCall(SemanticItem command)
        {
            var nativeAction = Db.GetNativeAction(command.Answer);
            if (nativeAction == null)
                return false;

            return nativeAction(command);
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

            var action1Result = ExecuteCommand(action1);
            var action2Result = action1Result && ExecuteCommand(action2);
            return action1Result && action2Result;
        }

        private bool _writeSlot(SemanticItem item)
        {
            var slot = item.GetSubstitutionValue("$slot");
            var value = item.GetSubstitution("$value");

            _slots[slot] = value;

            return true;
        }

        private SemanticItem _isSlotFilled(EvaluationContext context)
        {
            var slot = context.GetSubstitutionValue("$slot");
            var isFilled = _slots.ContainsKey(slot) && _slots[slot] != null;
            return isFilled ? SemanticItem.Yes : SemanticItem.No;
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

                if (evaluation.Question != Question.HowToEvaluate)
                    continue;

                if (evaluation.Input.Contains(something))
                {
                    referencedValue = evaluation.Result.First();
                }
            }

            return referencedValue;
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

        private SemanticItem _joinPhrases(EvaluationContext context)
        {
            var something1 = context.EvaluateOne("$something").Answer;
            var something2 = context.EvaluateOne("$something2").Answer;

            var joinedPhrase = something1 + " " + something2;
            return SemanticItem.Entity(joinedPhrase);
        }

        private bool _print(SemanticItem item)
        {
            var something = item.GetSubstitution("$something");

            Print(something.ReadableRepresentation());

            return true;
        }

        private bool _fireEvent(SemanticItem item)
        {
            var evt = item.GetSubstitutionValue("$event");
            FireEvent(evt);

            return true;
        }

        private bool _triggerAdd(SemanticItem item)
        {
            var action = item.GetSubstitutionValue("$action");
            var trigger = item.GetSubstitutionValue("$trigger");

            var constraints = new Constraints()
                .AddCondition(trigger);

            var actionItem = SemanticItem.From(Question.WhatShouldAgentDoNow, action, constraints);
            Db.Container.Add(actionItem);

            Log.SensorAdd(trigger, action);
            return true;
        }

        private bool _outputChanger(SemanticItem item)
        {
            var pattern = item.GetSubstitutionValue("$pattern");
            var replacement = item.GetSubstitutionValue("$replacement");

            _outputReplacements.Add(pattern, replacement);

            Print("ok");
            return true;
        }

        #region Body utilities

        internal void Print(string phrase)
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
