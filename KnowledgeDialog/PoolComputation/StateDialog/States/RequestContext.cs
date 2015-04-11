using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class RequestContext : StateBase
    {
        public readonly static StateProperty QuestionProperty = new StateProperty();

        public readonly static EdgeIdentifier HasContextAnswerEdge = new EdgeIdentifier();

        protected override ModifiableResponse execute()
        {
            var hasPossibleContext = Context.Pool.ActiveCount > 0;
            if (!hasPossibleContext)
                Context.SetValue(AcceptAdvice.IsBasedOnContextProperty, StateProperty.FalseValue);

            if (!Context.IsSet(AcceptAdvice.IsBasedOnContextProperty))
                return Response("I cannot fully understand your question. Are you asking for something connected with your previous question?");

            return EmitEdge(HasContextAnswerEdge);
        }
    }
}
