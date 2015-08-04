using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class AcceptAdvice : StateBase
    {
        public static readonly StateProperty2 CorrectAnswerProperty = new StateProperty2();
        public static readonly StateProperty2 IsBasedOnContextProperty = new StateProperty2();
        public static readonly EdgeIdentifier MissingInfoEdge = new EdgeIdentifier();
        public static readonly EdgeIdentifier MissingQuestionEdge = new EdgeIdentifier();
        public static readonly EdgeIdentifier AdviceAcceptedEdge = new EdgeIdentifier();

        protected override ModifiableResponse execute()
        {
            var unknownQuestion = Context.Get(RequestAnswer.QuestionProperty);
            if (unknownQuestion == null)
                unknownQuestion = Context.Get(QuestionAnswering.LastQuestion);

            var correctAnswer = Context.Get(CorrectAnswerProperty);
            var isBasedOnContext = Context.IsTrue(IsBasedOnContextProperty);
            var hasContextAnswer = Context.IsSet(IsBasedOnContextProperty);

            if (unknownQuestion == null || unknownQuestion.Trim() == "")
            {
                EmitEdge(MissingQuestionEdge);
                return Response("I don't understand your advice. Please give me some question.");
            }

            var needContextQuestion = Context.Pool.ActiveCount > 0;
            if (needContextQuestion && !hasContextAnswer)
            {
                //we don't know whether next answer is based on current pool or not
                return EmitEdge(MissingInfoEdge);
            }

            if (correctAnswer == null)
            {
                //we don't have context answer
                return EmitEdge(MissingInfoEdge);
            }

            var correctAnswerNode = Node(correctAnswer);
            Context.QuestionAnsweringModule.AdviceAnswer(unknownQuestion, isBasedOnContext, correctAnswerNode);

            Context.Pool.ClearAccumulator();
            Context.Pool.Insert(correctAnswerNode);

            Context.Remove(CorrectAnswerProperty);
            Context.Remove(IsBasedOnContextProperty);
            Context.Remove(RequestAnswer.QuestionProperty);

            EmitEdge(AdviceAcceptedEdge);
            return Response("Thank you");
        }
    }
}
