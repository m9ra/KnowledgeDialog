using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class AdviceRouting : StateBase
    {
        public readonly static StateProperty QuestionProperty = new StateProperty();

        protected override ModifiableResponse execute()
        {
            var hasPossibleContext = Context.Pool.ActiveCount > 0;
            if (!hasPossibleContext)
                Context.SetValue(AcceptAdvice.IsBasedOnContextProperty, StateProperty.FalseValue);

            if (!Context.IsSet(AcceptAdvice.IsBasedOnContextProperty))
                return Response("I cannot fully understand your question. Are you asking for something connected with your previous question?");


            if (!Context.IsSet(AcceptAdvice.CorrectAnswerProperty))
                return Response("Please, can you give me correct answer for your question?");

            throw new NotSupportedException("Current state is not supported");
        }
    }
}
