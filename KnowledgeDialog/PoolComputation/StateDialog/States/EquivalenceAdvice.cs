using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class EquivalenceAdvice : StateBase
    {
        public static readonly StateProperty IsEquivalent = new StateProperty();

        public static readonly EdgeIdentifier NoEquivalency = new EdgeIdentifier();

        protected override ModifiableResponse execute()
        {
            var isEquivalent = Context.IsTrue(IsEquivalent);
            var patternQuestion = Context.Get(EquivalenceQuestion.PatternQuestion);
            var queriedQuestion = Context.Get(EquivalenceQuestion.QueriedQuestion);

            Context.QuestionAnsweringModule.SetEquivalence(patternQuestion, queriedQuestion, isEquivalent);
            Context.SetValue(RequestAnswer.QuestionProperty, queriedQuestion);

            if (isEquivalent)
            {
                return EmitEdge(ForwardEdge);
            }
            else
            {
                return EmitEdge(NoEquivalency);
            }

        }
    }
}
