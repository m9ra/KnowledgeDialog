using PerceptiveDialogBasedAgent.V1.Interpretation;
using PerceptiveDialogBasedAgent.V1.Knowledge;
using PerceptiveDialogBasedAgent.V1.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V1
{
    delegate DbConstraint ParameterEvaluator(string parameter, EvaluationContext context);

    class Body
    {
        private readonly MindSet Mind;

        private readonly Dictionary<string, Sensor> _sensors = new Dictionary<string, Sensor>();

        private readonly Dictionary<string, Actor> _actors = new Dictionary<string, Actor>();

        private List<string> _inputHistory = new List<string>();

        private List<string> _outputHistory = new List<string>();

        private List<string> _outputCandidates = new List<string>();

        private List<DbConstraint> _topicHistory = new List<DbConstraint>();

        private List<DbConstraint> _searchResult = new List<DbConstraint>();

        private readonly Dictionary<string, NativeCallWrapper> _nativeCallWrappers = new Dictionary<string, NativeCallWrapper>();

        private DbConstraint _currentTopic;

        private ConstraintEntry _currentTopicPointer = null;

        #region Body control constants

        internal readonly string Agent = "agent";

        internal readonly string WhatToDoNowQ = "What should @ do now?";

        internal readonly string IsItFinishedQ = "Is @ finished?";

        internal readonly string WhatToDoNextQ = "What should @ do next?";

        internal readonly string TurnEndDurability = "until turn end";

        internal readonly string QuestionProcessedDurability = "until question processed";

        internal readonly string NativeMutliCallHandler = "%multi_call";

        #endregion

        internal Body(MindSet mind)
        {
            Mind = mind;

            //common abilities
            AddNativeAction(Print, "say", "$what");
            AddNativeAction(FormulateTopicQuestion, "formulate", "a", "topic", "question");
            AddNativeAction(AddSensorAction, RawEvaluator, ValueEvaluator, "add", "$action", "to", "$sensor");

            //topic handling
            AddNativeAction(WriteTopic, "write", "topic", "$topic");
            AddNativeAction(SetPointerToEmptyEntity, "set", "topic", "pointer", "to", "a", "missing", "entity");

            //search handling
            AddNativeAction(Search, "search", "$something");

            mind
                .AddPattern("input", "from", "user")
                    .HowToEvaluate(c => DbConstraint.Entity(_inputHistory.Last()))

                .AddPattern("topic")
                    .HowToEvaluate(c => _currentTopic)

                .AddPattern("a", "database", "question")
                    .HowToEvaluate(c => databaseQuestion())

                .AddPattern("yes")
                    .IsTrue(c => DbConstraint.Entity("yes"))

                .AddPattern("no")
                    .IsTrue(c => DbConstraint.Entity("no"))

                .AddPattern("first", "$action1", "second", "$action2", "third", "$action3")
                    .HowToDo(c =>
                    {
                        var action1 = c.Evaluate("action1", Evaluator.HowToDoQ);
                        var action2 = c.Evaluate("action2", Evaluator.HowToDoQ);
                        var action3 = c.Evaluate("action3", Evaluator.HowToDoQ);

                        return DbConstraint.Entity(NativeMutliCallHandler).ExtendByAnswer("1?", action1).ExtendByAnswer("2?", action2).ExtendByAnswer("3?", action3);
                    })

                .AddPattern("first", "$action1", "second", "$action2")
                    .HowToDo(c =>
                    {
                        var action1 = c.Evaluate("action1", Evaluator.HowToDoQ);
                        var action2 = c.Evaluate("action2", Evaluator.HowToDoQ);

                        return DbConstraint.Entity(NativeMutliCallHandler).ExtendByAnswer("1?", action1).ExtendByAnswer("2?", action2);
                    })
                ;

            _nativeCallWrappers.Add(NativeMutliCallHandler, MultiCall);
        }

        internal string Input(string utterance)
        {
            Log.DialogUtterance("U: " + utterance);

            //prepare agent for input
            Mind.Database.ClearFailingConstraints();
            _outputCandidates.Clear();

            //handle input processing
            _inputHistory.Add(utterance);
            if (_inputHistory.Count == 1)
                trigger("dialog begins");

            //run generated commands
            trigger("before input processing");
            runPolicy();
            trigger("after input processing");

            //handle output processing
            if (_outputCandidates.Count == 0)
                trigger("missing output");

            trigger("before output printing");
            var output = _outputCandidates.LastOrDefault();

            logDatabaseQuestions();

            // finish the turn 
            _outputHistory.Add(output);
            clearDatabase(TurnEndDurability);
            
            //TODO!!!!!!!!Clear What agent should do

            Log.DialogUtterance("S: " + output);
            return output;
        }

        internal Body Policy(string policyCommand)
        {
            //for policy adding, no history is considered
            Log.Policy(policyCommand);

            _inputHistory.Clear();
            _inputHistory.Add(policyCommand);

            trigger("before policy input processing");

            //run generated commands
            trigger("before input processing");
            runPolicy();
            trigger("after input processing");

            //cleanup
            clearDatabase(TurnEndDurability);
            _inputHistory.Clear();

            return this;
        }

        internal Body SensorAction(string sensor, string action)
        {
            AddSensorAction(action, sensor);
            return this;
        }

        #region Processing utilities

        private void trigger(string trigger)
        {
            var sensor = getSensor(trigger);

            foreach (var action in sensor.Actions)
            {
                Execute(action);
            }
        }

        private Sensor getSensor(string trigger)
        {
            if (!_sensors.TryGetValue(trigger, out var sensor))
                _sensors[trigger] = sensor = new Sensor(trigger);
            return sensor;
        }

        private void runPolicy()
        {
            //determine what agent should do
            var whatToDo = Answer(Agent, WhatToDoNowQ);
            if (whatToDo == null)
            {
                //we have no action yet
                trigger("missing policy action");
                whatToDo = Answer(Agent, WhatToDoNowQ);
            }

            //do requested action
            if (!Execute(whatToDo))
                return;
        }

        #endregion

        #region Native body API

        protected DbConstraint MultiCall(DbConstraint call)
        {
            var arguments = call.SubjectConstraints.OrderBy(c => c.Question.Length).OrderBy(c => c.Question).ToArray();
            foreach (var argument in arguments)
            {
                var callResult = executeCall(argument.Answer);
                if (!callResult)
                    return DbConstraint.Entity("fail");
            }

            return DbConstraint.Entity("success");
        }

        protected void WriteTopic(DbConstraint topic)
        {
            _currentTopic = topic;
        }

        protected void SetPointerToEmptyEntity()
        {
            foreach (var entry in _currentTopic.Entries)
            {
                if (entry.Answer == null)
                {
                    _currentTopicPointer = entry;
                    break;
                }
            }
        }

        protected void Search(DbConstraint constraint)
        {
            throw new NotImplementedException();
        }

        protected void Print(string what)
        {
            _outputCandidates.Add(what);
        }

        protected void FormulateTopicQuestion()
        {
            if (_currentTopicPointer == null)
                throw new NotImplementedException();

            var question = formulateQuestion(new DbConstraint(_currentTopicPointer));
            _outputCandidates.Add(question);
        }

        protected void AddSensorAction(string action, string sensorTrigger)
        {
            Log.AddingSensorAction(sensorTrigger, action);

            var sensor = getSensor(sensorTrigger);
            sensor.Actions.Add(action);
        }

        #endregion

        #region Capability handling

        protected bool Execute(string command)
        {
            if (command == null)
                return false;


            var evaluatedCommand = Evaluate(command, Evaluator.HowToDoQ);
            Log.Execution(command, evaluatedCommand);

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

        private void logDatabaseQuestions()
        {
            var questions = Mind.Database.FailingConstraints.Select(q => formulateQuestion(q)).ToArray();

            Log.List("DATABASE QUESTIONS", questions);
        }

        private void clearDatabase(string durability)
        {
            Mind.Database.ClearEntriesWith(durability);
        }

        private DbConstraint databaseQuestion()
        {
            var failingConstraints = Mind.Database.FailingConstraints.ToArray();
            if (failingConstraints.Length == 0)
                throw new NotImplementedException();

            return failingConstraints.Reverse().First();
        }

        private string formulateQuestion(DbConstraint constraint)
        {
            var questionEntry = createQuestionEntry(constraint);
            if (questionEntry == null)
                throw new NotImplementedException();

            var questionSubject = questionEntry.Subject.PhraseConstraint;
            var questionStr = questionEntry.Question;
            var readableQuestion = string.Format(questionStr.Replace("@", "{0}"), questionSubject);

            return readableQuestion;

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
            {
                trigger("missing call handler");
                return false;
            }

            var call = _nativeCallWrappers[callName];
            call(callEntity);

            return true;
        }

        private NativePhraseEvaluator CreateActionSemantic(Action<string, string> action, ParameterEvaluator param1, ParameterEvaluator param2, string actionName, string parameterName1, string parameterName2)
        {
            var nativeCallWrapperId = "%" + actionName;
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
                    .ExtendByAnswer(parameterName1, param1(parameterName1, c))
                    .ExtendByAnswer(parameterName2, param2(parameterName2, c))
                    ;
            };
        }


        private NativePhraseEvaluator CreateActionSemantic(Action<string> action, string actionName, string parameterName1)
        {
            var nativeCallWrapperId = "%" + actionName;
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

        private NativePhraseEvaluator CreateActionSemantic(Action<DbConstraint> action, string actionName, string parameterName1)
        {
            var nativeCallWrapperId = "%" + actionName;
            NativeCallWrapper nativeCallWrapper = i =>
            {
                var argumentEntity1 = i.GetSubjectConstraint(parameterName1);
                action(argumentEntity1);
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
            var nativeCallWrapperId = "%" + actionName;
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

        internal void AddNativeAction(Action<string, string> action, ParameterEvaluator param1, ParameterEvaluator param2, params string[] pattern)
        {
            var variables = pattern.Where(w => w.StartsWith("$")).ToArray();
            if (variables.Length != 2)
                throw new NotImplementedException();
            var variable1 = variables[0].Substring(1);
            var variable2 = variables[1].Substring(1);
            Mind
               .AddPattern(pattern)
                   .HowToDo(CreateActionSemantic(action, param1, param2, string.Join("_", pattern), variable1, variable2));
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

        internal void AddNativeAction(Action<DbConstraint> action, params string[] pattern)
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

        internal DbConstraint ValueEvaluator(string parameter, EvaluationContext context)
        {
            return context.Evaluate(parameter, Evaluator.HowToEvaluateQ);
        }

        internal DbConstraint RawEvaluator(string parameter, EvaluationContext context)
        {
            return context.Raw(parameter);
        }

        internal DbConstraint ActionEvaluator(string parameter, EvaluationContext context)
        {
            return context.Evaluate(parameter, Evaluator.HowToDoQ);
        }

        #endregion
    }
}
