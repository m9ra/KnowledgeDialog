using PerceptiveDialogBasedAgent.Interpretation;
using PerceptiveDialogBasedAgent.Knowledge;
using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent
{
    class EmptyAgent
    {
        protected readonly MindSet Mind = new MindSet();

        private readonly Dictionary<string, NativeCallWrapper> _nativeCallWrappers = new Dictionary<string, NativeCallWrapper>();

        private readonly string _agent = "agent";

        private string _currentGoal = null;

        private string _lastQuestionSubject = null;

        private string _lastQuestion = null;

        internal readonly string WhatToDoNowQ = "What should @ do now?";

        internal readonly string IsItFinishedQ = "Is @ finished?";

        internal readonly string WhatToDoNextQ = "What should @ do next?";

        internal readonly string WhatToCheckQ = "What should @ check?";

        internal readonly string WhatToOutputQ = "What should @ output?";

        internal EmptyAgent()
        {
            Mind
                .AddFact(_agent, WhatToCheckQ, "nothing") //by default there is nothing check (to prevent DB from failing constraints here)

                .AddPattern("say", "$what")
                    .HowToDo(CreateActionSemantic(Print, "Print", "what"));

        }

        internal string Input(string data)
        {
            Mind.Database.ClearFailingConstraints();
            Mind.Database.AddFact("user", "What @ said?", data);

            //run sensoring logic of the agent
            foreach (var sensor in AnswerWithMany(_agent, WhatToCheckQ))
            {
                //TODO evaluate sensors
            }

            if (_lastQuestion != null)
            {
                //TODO we should care about conditions
                Mind.AddFact(_lastQuestionSubject, _lastQuestion, data);
                _lastQuestion = null;
                _lastQuestionSubject = null;
            }

            //determine what agent should do
            var whatToDo = Answer(_agent, WhatToDoNowQ);
            if (whatToDo == null)
            {
                if (_currentGoal != null)
                {
                    var isCurrentGoalFinished = IsTrue(Answer(_currentGoal, IsItFinishedQ));
                    if (isCurrentGoalFinished)
                        _currentGoal = whatToDo = Answer(_currentGoal, WhatToDoNextQ);
                    else
                        whatToDo = _currentGoal;
                }
            }

            //do requested action
            var command = Evaluate(whatToDo, Evaluator.HowToDoQ);
            if (!executeCall(command))
                outputQuestion();

            //collect output
            var output = Answer(_agent, WhatToOutputQ);

            //TODO make more systematic cleanup
            Mind.Database.RemoveFact("user", "What @ said?", data);
            Mind.Database.RemoveFact(_agent, WhatToOutputQ, output);

            return output;
        }

        protected string Answer(string subject, string question)
        {
            return AnswerWithMany(subject, question).FirstOrDefault();
        }

        protected string[] AnswerWithMany(string subject, string question)
        {
            var result = new List<string>();
            foreach (var row in Query(new ConstraintEntry(DbConstraint.Entity(subject), question, null)))
            {
                result.Add(row.Substitution);
            }

            return result.ToArray();
        }

        protected bool IsTrue(string subject)
        {
            throw new NotImplementedException();
        }

        protected DbConstraint Evaluate(string subject, string evaluationQuestion)
        {
            if (subject == null)
                return null;

            var result = Mind.Evaluator.Evaluate(subject, evaluationQuestion);
            return result.Constraint;
        }

        protected DbResult[] Query(params ConstraintEntry[] entries)
        {
            return Mind.Database.Query(new DbConstraint(entries)).ToArray();
        }

        protected DbConstraint IfDbContains(DbConstraint constraint)
        {
            return Mind.Database.Query(constraint).Any() ? DbConstraint.Entity("true") : DbConstraint.Entity("false");

        }

        internal void AddPolicyFact(string act)
        {
            throw new NotImplementedException();
        }

        private void outputQuestion()
        {
            var failingConstraints = Mind.Database.FailingConstraints.ToArray();
            if (failingConstraints.Length == 0)
                throw new NotImplementedException();

            foreach (var constraint in failingConstraints.Reverse())
            {
                var questionEntry = createQuestionEntry(constraint);
                if (questionEntry == null)
                    continue;

                _lastQuestionSubject = questionEntry.Subject.PhraseConstraint;
                _lastQuestion = questionEntry.Question;
                var question = string.Format(_lastQuestion.Replace("@", "{0}"), _lastQuestionSubject);

                Mind.AddFact(_agent, WhatToOutputQ, question);
                return;
            }

            throw new NotImplementedException("Cannot create a question");
        }

        private ConstraintEntry createQuestionEntry(DbConstraint constraint)
        {
            if (constraint.SubjectConstraints.Any())
                throw new NotImplementedException();

            var answerConstraints = constraint.AnswerConstraints.ToArray();
            if (answerConstraints.Length != 1)
                throw new NotImplementedException();

            return answerConstraints[0];
        }

        private bool executeCall(DbConstraint callEntity)
        {
            if (callEntity == null)
                return false;

            var callName = callEntity.PhraseConstraint;
            if (callName == "nothing")
                return false;

            if (!_nativeCallWrappers.ContainsKey(callName))
                return false;

            var call = _nativeCallWrappers[callName];
            call(callEntity);

            return true;
        }

        private NativePhraseEvaluator CreateActionSemantic(Action<string> action, string actionName, string parameterName)
        {
            var nativeCallWrapperId = ".native_executor-" + actionName;
            NativeCallWrapper nativeCallWrapper = i =>
            {
                var argumentEntity = i.GetSubjectConstraint(parameterName);
                action(argumentEntity.PhraseConstraint);
                return null;
            };

            _nativeCallWrappers.Add(nativeCallWrapperId, nativeCallWrapper);

            return c =>
            {
                var callEntity = DbConstraint.Entity(nativeCallWrapperId);
                return callEntity.ExtendByAnswer(parameterName, c[parameterName]);
            };
        }

        protected void Print(string what)
        {
            Mind.AddFact(_agent, WhatToOutputQ, what);
        }
    }
}
