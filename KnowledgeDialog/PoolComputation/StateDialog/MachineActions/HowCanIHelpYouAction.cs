using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class HowCanIHelpYouAction : MachineActionBase
    {
        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return !InputState.HasNonAnsweredQuestion && !InputState.HasAdvice && !InputState.HasUnknownQuestion;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            EmitResponse("How can I help you?");
        }
    }
}