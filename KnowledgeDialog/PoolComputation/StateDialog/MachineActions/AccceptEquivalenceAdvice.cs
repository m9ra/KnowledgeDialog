using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class AcceptEquivalenceAdvice : MachineActionBase
    {
        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return InputState.HasNonAnsweredQuestion && InputState.HasEquivalenceCandidate && InputState.HasConfirmation;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            InputState.QA.SetEquivalence(InputState.EquivalenceCandidate.OriginalSentence,InputState.Question.OriginalSentence,InputState.HasAffirmation);

            RemoveConfirmation();
            RemoveEquivalenceCandidate();

            if (InputState.HasNegation)
            {
                //we don't know the correct answer
                //mark the question as unknown and let it process further
                SetQuestionAsUnknown();
            }

            //we have to process the the state further
            ForwardControl();
        }
    }
}