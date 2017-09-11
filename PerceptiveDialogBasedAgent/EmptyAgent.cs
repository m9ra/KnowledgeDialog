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

        private readonly DbConstraint _agent = DbConstraint.Entity("agent");

        private readonly List<DbConstraint> _inputActionsDone = new List<DbConstraint>();

        internal readonly string WhatToDoQ = "What @ should do?";

        internal readonly string WhatToCheckQ = "What @ should check?";

        internal readonly string WhatToOutputQ = "What @ should output?";

        internal EmptyAgent()
        {
            Mind
                .AddPattern("say", "$what")
                    .HowToEvaluate(CreateActionSemantic(Print, "Print", "what"));
        }

        internal string Input(string data)
        {
            Mind.Database.AddFact("user", "What @ said?", data);

            foreach (var sensor in Query(new ConstraintEntry(_agent, WhatToCheckQ, null)))
            {
                //TODO evaluate sensors
            }

            var whatToDo = Query(new ConstraintEntry(_agent, WhatToDoQ, null)).ToArray();
            foreach (var answer in whatToDo)
            {
                var result = Mind.Evaluator.Evaluate(answer.Substitution, Evaluator.HowToDoQ);
                if (!executeCall(result.Constraint))
                    throw new NotImplementedException("How to do");

                break;
            }

            if (whatToDo.Length == 0)
            {
                outputQuestion(WhatToDoQ);
            }

            Mind.Database.RemoveFact("user", "What @ said?", data);

            var whatToOutput = Mind.Database.GetAnswers(_agent.PhraseConstraint, WhatToOutputQ);
            return whatToOutput.Last();
        }

        private void outputQuestion(string question)
        {
            Mind.Database.AddFact(_agent.PhraseConstraint, WhatToOutputQ, question);
        }

        protected IEnumerable<DbResult> Query(params ConstraintEntry[] entries)
        {
            return Mind.Database.Query(new DbConstraint(entries));
        }

        protected DbConstraint IfDbContains(DbConstraint constraint)
        {
            return Mind.Database.Query(constraint).Any() ? DbConstraint.Entity("true") : DbConstraint.Entity("false");

        }

        internal void AddPolicyFact(string act)
        {
            Mind.AddFact(_agent.PhraseConstraint, WhatToDoQ, act);
        }

        private bool executeCall(DbConstraint callEntity)
        {
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
            Console.WriteLine(what);
        }
    }
}
