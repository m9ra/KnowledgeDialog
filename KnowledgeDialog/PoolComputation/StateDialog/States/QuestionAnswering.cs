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
        public static readonly StateProperty2 LastQuestion = new StateProperty2();

        public static readonly EdgeIdentifier QuestionAnswered = new EdgeIdentifier();

        protected override ModifiableResponse execute()
        {
            var question = Context.Get(RequestAnswer.QuestionProperty);
            if (question == null)
                throw new NotSupportedException("Cannot provide answer without question");

            Context.SetValue(LastQuestion, question);

            var parsedQuestion = UtteranceParser.Parse(question);
            var answer = Context.QuestionAnsweringModule.GetAnswer(parsedQuestion).ToArray();

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
                //TODO find criterion
                response = Response("There is too much information for output. Please be more specific");
            }


            Context.Remove(RequestAnswer.QuestionProperty);

            EmitEdge(QuestionAnswered);
            return response;
        }

     
    }
}
