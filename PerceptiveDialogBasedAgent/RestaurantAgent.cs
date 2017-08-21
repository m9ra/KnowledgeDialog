using PerceptiveDialogBasedAgent.Interpretation;
using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent
{
    internal delegate DbConstraint NativeCallWrapper(DbConstraint input);

    class RestaurantAgent
    {
        private readonly MindSet _mind = new MindSet();

        private readonly Dictionary<string, NativeCallWrapper> _nativeCallWrappers = new Dictionary<string, NativeCallWrapper>();

        private readonly DbConstraint _agentAction = DbConstraint.Entity("agent action");

        internal readonly string WhatToDoQ = "What @ should do?";

        internal RestaurantAgent()
        {
            _mind
                .AddPattern("if", "$something", "then", "$something2")
                    .Semantic(c =>
                    {
                        var isConditionTrue = c.IsTrue(c["something"]);
                        if (isConditionTrue)
                        {
                            return c["something2"];
                        }
                        throw new NotImplementedException("represent non-satisfied condition");
                    })

                .AddPattern("$someone", "said", "$something")
                    .Semantic(c => c["someone"].ExtendByAnswer("What @ said?", c["something"]))

                .AddPattern("say", "$what")
                    .Semantic(createActionSemantic(Print, "Print", "what"))

                ;

            _mind.AddFact(_agentAction.PhraseConstraint, WhatToDoQ, "if user said hello then say hi");
        }

        internal void Input(string data)
        {
            _mind.Database.AddFact("user", "What @ said?", data);

            foreach (var answer in _mind.Database.Query(new DbConstraint(new ConstraintEntry(_agentAction, WhatToDoQ, null))))
            {
                var result = _mind.Evaluator.Evaluate(answer.Substitution);
                executeCall(result.Constraint);
            }
        }

        private void executeCall(DbConstraint callEntity)
        {
            var call = _nativeCallWrappers[callEntity.PhraseConstraint];
            call(callEntity);
        }

        private NativePhraseEvaluator createActionSemantic(Action<string> action, string actionName, string parameterName)
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

        private void Print(string what)
        {
            Console.WriteLine(what);
        }
    }
}
