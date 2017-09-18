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


    class Body
    {
        private readonly MindSet Mind;

        private readonly Dictionary<string, Sensor> _sensors = new Dictionary<string, Sensor>();

        private readonly Dictionary<string, Actor> _actors = new Dictionary<string, Actor>();

        private List<string> _inputHistory = new List<string>();

        private List<string> _outputHistory = new List<string>();

        private List<string> _outputCandidates = new List<string>();

        private readonly Dictionary<string, NativeCallWrapper> _nativeCallWrappers = new Dictionary<string, NativeCallWrapper>();

        private string _currentGoal = null;

        private string _lastQuestionSubject = null;

        private string _lastQuestion = null;

        #region Body control constants

        internal readonly string Agent = "agent";

        internal readonly string WhatToDoNowQ = "What should @ do now?";

        internal readonly string IsItFinishedQ = "Is @ finished?";

        internal readonly string WhatToDoNextQ = "What should @ do next?";

        internal readonly string TurnEndDurability = "until turn end";

        internal readonly string QuestionProcessedDurability = "until question processed";

        #endregion

        internal Body(MindSet mind)
        {
            Mind = mind;

            AddNativeAction(Print, "say", "$what");
            AddNativeAction(AskForAdvice, "ask", "for", "advice");
            AddNativeAction(AddSensorAction, "add", "$action", "to", "$sensor");

            mind
                .AddPattern("input", "from", "user")
                    .HowToEvaluate(c => DbConstraint.Entity(_inputHistory.Last()))


                ;
        }

        internal string Input(string utterance)
        {
            Mind.Database.ClearFailingConstraints();
            _outputCandidates.Clear();

            _inputHistory.Add(utterance);
            if (_inputHistory.Count == 0)
                runSensor("dialog begins");

            runSensor("before input processing");
            runPolicy();
            runSensor("after input processing");

            runSensor("before output printing");
            var output = _outputCandidates.LastOrDefault();
            _outputHistory.Add(output);
            return output;
        }

        #region Processing utilities

        private void runSensor(string trigger)
        {
            if (!_sensors.TryGetValue(trigger, out var sensor))
                _sensors[trigger] = sensor = new Sensor(trigger);

            foreach (var action in sensor.Actions)
            {
                Execute(action);
            }
        }

        private void runPolicy()
        {
            //determine what agent should do
            var whatToDo = Answer(Agent, WhatToDoNowQ);
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

            throw new NotImplementedException("Handle question asking");

            //do requested action
            if (!Execute(whatToDo))
                outputQuestion();

            clearDatabase(TurnEndDurability);
        }

        #endregion

        #region Native body API

        protected void Print(string what)
        {
            _outputCandidates.Add(what);
        }

        protected void AskForAdvice()
        {
            throw new NotImplementedException();
        }

        protected void AddSensorAction(string action, string sensor)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Capability handling

        protected bool Execute(string command)
        {
            var evaluatedCommand = Evaluate(command, Evaluator.HowToDoQ);
            return executeCall(evaluatedCommand);
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

        internal DbConstraint IfDbContains(DbConstraint constraint)
        {
            return Mind.Database.Query(constraint).Any() ? DbConstraint.Entity("true") : DbConstraint.Entity("false");

        }

        internal void AddPolicyFact(string act)
        {
            throw new NotImplementedException();
        }

        private void clearDatabase(string durability)
        {
            Mind.Database.ClearEntriesWith(durability);
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

                Print(question);
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

        private NativePhraseEvaluator CreateActionSemantic(Action<string, string> action, string actionName, string parameterName1, string parameterName2)
        {
            var nativeCallWrapperId = ".native_executor-" + actionName;
            NativeCallWrapper nativeCallWrapper = i =>
            {
                var argumentEntity1 = i.GetSubjectConstraint(parameterName1);
                var argumentEntity2 = i.GetSubjectConstraint(parameterName2);
                action(argumentEntity1.PhraseConstraint, argumentEntity2.PhraseConstraint);
                return null;
            };

            _nativeCallWrappers.Add(nativeCallWrapperId, nativeCallWrapper);

            return c =>
            {
                var callEntity = DbConstraint.Entity(nativeCallWrapperId);
                return callEntity
                    .ExtendByAnswer(parameterName1, c[parameterName1])
                    .ExtendByAnswer(parameterName2, c[parameterName2])
                    ;
            };
        }

        private NativePhraseEvaluator CreateActionSemantic(Action<string> action, string actionName, string parameterName1)
        {
            var nativeCallWrapperId = ".native_executor-" + actionName;
            NativeCallWrapper nativeCallWrapper = i =>
            {
                var argumentEntity1 = i.GetSubjectConstraint(parameterName1);
                action(argumentEntity1.PhraseConstraint);
                return null;
            };

            _nativeCallWrappers.Add(nativeCallWrapperId, nativeCallWrapper);

            return c =>
            {
                var callEntity = DbConstraint.Entity(nativeCallWrapperId);
                return callEntity
                    .ExtendByAnswer(parameterName1, c[parameterName1])
                    ;
            };
        }

        private NativePhraseEvaluator CreateActionSemantic(Action action, string actionName)
        {
            var nativeCallWrapperId = ".native_executor-" + actionName;
            NativeCallWrapper nativeCallWrapper = i =>
            {
                action();
                return null;
            };

            _nativeCallWrappers.Add(nativeCallWrapperId, nativeCallWrapper);

            return c =>
            {
                var callEntity = DbConstraint.Entity(nativeCallWrapperId);
                return callEntity;
            };
        }

        internal void AddNativeAction(Action<string, string> action, params string[] pattern)
        {
            var variables = pattern.Where(w => w.StartsWith("$")).ToArray();
            if (variables.Length != 2)
                throw new NotImplementedException();
            var variable1 = variables[0].Substring(1);
            var variable2 = variables[1].Substring(1);
            Mind
               .AddPattern(pattern)
                   .HowToDo(CreateActionSemantic(action, string.Join("_", pattern), variable1, variable2));
        }

        internal void AddNativeAction(Action<string> action, params string[] pattern)
        {
            var variables = pattern.Where(w => w.StartsWith("$")).ToArray();
            if (variables.Length != 1)
                throw new NotImplementedException();
            var variable = variables[0].Substring(1);
            Mind
               .AddPattern(pattern)
                   .HowToDo(CreateActionSemantic(action, string.Join("_", pattern), variable));
        }

        internal void AddNativeAction(Action action, params string[] pattern)
        {
            var variables = pattern.Where(w => w.StartsWith("$")).ToArray();
            if (variables.Length != 0)
                throw new NotImplementedException();

            Mind
               .AddPattern(pattern)
                   .HowToDo(CreateActionSemantic(action, string.Join("_", pattern)));
        }

        #endregion
    }
}
