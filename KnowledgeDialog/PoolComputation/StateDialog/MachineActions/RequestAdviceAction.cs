using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class RequestAdviceAction : MachineActionBase
    {
        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return InputState.HasUnknownQuestion && !InputState.HasAdvice && !ActionIsPending;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            EmitResponse("I don't know. What is the correct answer please?");
            MarkActionAsPending();   
        }
    }
}