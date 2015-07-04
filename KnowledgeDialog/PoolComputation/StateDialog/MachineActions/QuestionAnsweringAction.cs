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
        internal static double AnswerConfidenceThreshold = 0.9;

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
                RemoveQuestion();
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
            var answerHypothesis = InputState.QA.GetBestHypothesis(question);
            if (answerHypothesis == null)
                //we don't know anything about the question
                return null;

            if (answerHypothesis.Control.Score < AnswerConfidenceThreshold)
                //sentences are not similar enough
                return null;

            var answerNodes = InputState.QA.GetAnswer(answerHypothesis);
            if (answerNodes == null)
                //cannot find the node
                return null;

            var joinedAnswer = string.Join(" and ", answerNodes.Select(a => a.Data));
            return string.Format("It is {0}.", joinedAnswer);
        }

        private ParsedExpression getEquivalenceCandidate(ParsedExpression question)
        {
            var bestHypothesis = InputState.QA.GetBestHypothesis(question);
            if (bestHypothesis == null)
                return null;

            throw new NotImplementedException();
        }
    }
}