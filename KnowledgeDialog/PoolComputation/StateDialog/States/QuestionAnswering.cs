using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class QuestionAnswering : StateBase
    {
        public static readonly StateProperty LastQuestion = new StateProperty();

        public static readonly EdgeIdentifier QuestionAnswered = new EdgeIdentifier();

        protected override ModifiableResponse execute()
        {
            var question = Context.Get(AdviceRouting.QuestionProperty);
            if (question == null)
                throw new NotSupportedException("Cannot provide answer without question");

            Context.SetValue(LastQuestion, question);

            var answer = Context.QuestionAnsweringModule.GetAnswer(question).ToArray();

            ModifiableResponse response;
            if (answer.Length <= Context.MaximumUserReport)
            {
                Context.Pool.ClearAccumulator();
                Context.Pool.Insert(answer);
                if (Context.Pool.HasActive)
                {
                    response = Response("It is", Context.Pool.ActiveNodes);
                }
                else
                {
                    response = Response("I don't know.");
                }
            }
            else
            {
                throw new NotImplementedException("Find criterion");
            }


            Context.Remove(AdviceRouting.QuestionProperty);

            EmitEdge(QuestionAnswered);
            return response;
        }

     
    }
}
