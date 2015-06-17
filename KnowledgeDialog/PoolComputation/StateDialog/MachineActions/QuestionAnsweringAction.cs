using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class QuestionAnsweringAction : MachineActionBase
    {
        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return InputState.HasNonAnsweredQuestion;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            var answer = getAnswer(InputState.Question);
            if (answer != null)
            {
                //we have answer, let present it
                EmitResponse(answer);
                MarkQuestionAnswered();
                return;
            }

            var equivalenceCandidate = getEquivalenceCandidate(InputState.Question);
            if (equivalenceCandidate != null)
            {
                //we don't know how to answer the question
                //however we know some similar question
                SetEquivalenceHypothesis(InputState.Question, equivalenceCandidate);
                RemoveQuestion();

                //state has to be processed further - we are forwarding
                //control into another machine action
                ForwardControl();
                return;
            }

            //We don't know the answer - we let the state to be processed further
            SetQuestionAsUnknown();
            ForwardControl();
        }

        private string getAnswer(ParsedExpression question)
        {
            throw new NotImplementedException();
        }

        private ParsedExpression getEquivalenceCandidate(ParsedExpression question)
        {
            throw new NotImplementedException();
        }
    }
}