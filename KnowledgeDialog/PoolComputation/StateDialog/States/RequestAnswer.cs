using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class RequestAnswer : StateBase
    {
        public readonly static StateProperty2 QuestionProperty = new StateProperty2();

        public readonly static EdgeIdentifier HasCorrectAnswerEdge = new EdgeIdentifier();

        protected override ModifiableResponse execute()
        {
            var hasPossibleContext = Context.Pool.ActiveCount > 0;
            if (!hasPossibleContext)
                Context.SetValue(AcceptAdvice.IsBasedOnContextProperty, StateProperty2.FalseValue);
                      
            if (!Context.IsSet(AcceptAdvice.CorrectAnswerProperty))
                return Response("I don't understand, can you give me correct answer for your question please?");

            return EmitEdge(HasCorrectAnswerEdge);
        }
    }
}
