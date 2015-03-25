using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class RepairAnswer : StateBase
    {
        public readonly static EdgeIdentifier NoQuestionForRepair = new EdgeIdentifier();

        public readonly static EdgeIdentifier AdviceAccepted = new EdgeIdentifier();

        protected override ModifiableResponse execute()
        {
            var repairedQuestion = Context.Get(QuestionAnswering.LastQuestion);
            var correctAnswer = Context.Get(AcceptAdvice.CorrectAnswerProperty);
            if (repairedQuestion == null)
            {
                EmitEdge(NoQuestionForRepair);
                return Response("Sorry, I don't know what are you talking about.");
            }

            Context.QuestionAnsweringModule.SuggestAnswer(repairedQuestion, Context.Graph.GetNode(correctAnswer));

            Context.Remove(AcceptAdvice.CorrectAnswerProperty);
            return EmitEdge(AdviceAccepted);

        }
    }
}
