using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class Body
    {
        internal readonly static string WhatShouldAgentDoNowQ = "what should agent do now ?";

        internal readonly static string HowToDoQ = "how to do $@ ?";

        internal readonly static string IsItTrueQ = "is $@ true ?";

        internal readonly static string HowToEvaluateQ = "how to evaluate $@ ?";

        internal readonly static string UserInputVar = "$user_input";

        internal readonly Database Db = new EvaluatedDatabase();

        private readonly List<string> _outputCandidates = new List<string>();

        private readonly List<string> _inputHistory = new List<string>();

        private readonly Stack<string> _scopes = new Stack<string>();

        private string _currentPattern;

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

            //handle output processing
            pushScope("output printing");
            var output = _outputCandidates.LastOrDefault();
            popScope("output printing");
            popScope("turn");

            var log = Db.FinishLog();

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
            runPolicy();
            popScope("input processing");

            popScope("policy");

            var log = Db.FinishLog();
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

        public Body IsTrue(string description)
        {
            Db.Add(SemanticItem.Pattern(_currentPattern, IsItTrueQ, description));
            return this;
        }

        private void runPolicy()
        {
            var commands = getAnswers(WhatShouldAgentDoNowQ);

            foreach (var command in commands)
            {
                if (!executeCall(command))
                    //something went wrong, the evaluation will be stopped
                    throw new NotImplementedException();
            }

            handleMissingOutput();
        }

        private void handleMissingOutput()
        {
            pushScope("missing output");
            //TODO how to react?
            popScope("missing output");
        }

        private bool executeCall(SemanticItem command)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<SemanticItem> getAnswers(string question)
        {
            var currentConstraints = createConstraintValues();
            var queryItem = SemanticItem.AnswerQuery(question, currentConstraints);

            var result = Db.Query(queryItem).ToArray();
            return result;
        }

        private Constraints createConstraintValues()
        {
            return new Constraints()
                .AddValue(UserInputVar, _inputHistory[0])
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
    }
}
