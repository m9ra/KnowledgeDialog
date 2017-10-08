using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    internal delegate bool NativeAction(SemanticItem input);

    class Body
    {
        internal readonly static string WhatShouldAgentDoNowQ = "what should agent do now ?";

        internal readonly static string HowToDoQ = "how to do $@ ?";

        internal readonly static string IsItTrueQ = "is $@ true ?";

        internal readonly static string HowToEvaluateQ = "how to evaluate $@ ?";

        internal readonly static string UserInputVar = "$user_input";

        internal readonly EvaluatedSpanDatabase Db = new EvaluatedSpanDatabase();

        private readonly List<string> _outputCandidates = new List<string>();

        private readonly List<string> _inputHistory = new List<string>();

        private readonly Stack<string> _scopes = new Stack<string>();

        private readonly Dictionary<string, NativeAction> _nativeActions = new Dictionary<string, NativeAction>();

        private string _currentPattern;

        internal IEnumerable<string> InputHistory => _inputHistory;

        internal Body()
        {
            this
                .Pattern("add $action to trigger $trigger")
                    .HowToDo("TriggerAdd", _triggerAdd)

                .Pattern("print $something")
                    .HowToDo("Print", _print)

                .Pattern("user input")
                    .HowToEvaluate("UserInput", _userInput)
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
                pushScope("dialog");

            pushScope("turn");

            pushScope("input processing");
            runPolicy();
            popScope("input processing");

            var log = Db.FinishLog();
            var questions = log.GetQuestions();

            //handle output processing
            pushScope("output printing");
            var output = _outputCandidates.LastOrDefault();
            popScope("output printing");
            popScope("turn");

            Log.Questions(questions);

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
            pushScope("policy");

            pushScope("input processing");
            executeCommand(utterance);
            popScope("input processing");

            popScope("policy");

            var log = Db.FinishLog();
            var questions = log.GetQuestions();
            Log.Questions(questions);

            // policy wont keep any history
            _inputHistory.Clear();
            _outputCandidates.Clear();
        }


        public Body Pattern(string pattern)
        {
            _currentPattern = pattern;

            return this;
        }

        public Body HowToDo(string description)
        {
            Db.Add(SemanticItem.Pattern(_currentPattern, HowToDoQ, description));
            return this;
        }


        public Body HowToEvaluate(string description)
        {
            Db.Add(SemanticItem.Pattern(_currentPattern, HowToEvaluateQ, description));
            return this;
        }

        public Body HowToDo(string evaluatorName, NativeEvaluator evaluator)
        {
            var evaluatorId = $"%{evaluatorName}-how_to_do";
            HowToDo(evaluatorId);

            Db.AddEvaluator(evaluatorId, evaluator);

            return this;
        }

        public Body HowToDo(string actionName, NativeAction action)
        {
            var evaluatorId = "#" + actionName;
            HowToDo(evaluatorId);

            _nativeActions.Add(evaluatorId, action);
            Db.AddSpanElement(evaluatorId);

            return this;
        }

        public Body HowToEvaluate(string evaluatorName, NativeEvaluator evaluator)
        {
            var evaluatorId = $"%{evaluatorName}-how_to_evaluate";
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
            var evaluatorId = $"%{evaluatorName}-is_true";
            IsTrue(evaluatorId);

            Db.AddEvaluator(evaluatorId, evaluator);

            return this;
        }

        private bool executeCommand(string utterance)
        {
            return executeCommand(SemanticItem.Entity(utterance));
        }

        private bool executeCommand(SemanticItem command)
        {
            var commandQuery = SemanticItem.AnswerQuery(Body.HowToDoQ, Constraints.WithInput(command));
            var commandInterpretations = Db.Query(commandQuery).ToArray();

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

        private void runPolicy()
        {
            var commands = getAnswers(WhatShouldAgentDoNowQ);

            foreach (var command in commands)
            {
                if (!executeCommand(command))
                    //something went wrong, the evaluation will be stopped
                    //TODO handle the failure somehow
                    break;
            }

            handleMissingOutput();
        }

        private void handleMissingOutput()
        {
            if (_outputCandidates.Count > 0)
                return;

            pushScope("missing output");
            //TODO how to react?
            popScope("missing output");
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
            return new Constraints()
                .AddInput(_inputHistory[0])
                ;
        }

        private void pushScope(string scope)
        {
            _scopes.Push(scope);
        }

        private void popScope(string scope)
        {
            var poppedScope = _scopes.Pop();
            if (poppedScope != scope)
                throw new InvalidOperationException("Cannot pop givens cope");
        }

        private SemanticItem _userInput(EvaluationContext context)
        {
            return SemanticItem.Entity(_inputHistory.Last());
        }

        private bool _print(SemanticItem item)
        {
            _outputCandidates.Add(item.GetSubstitutionValue("$something"));
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
    }
}
