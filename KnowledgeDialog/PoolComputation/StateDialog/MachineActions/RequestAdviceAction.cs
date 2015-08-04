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
            return InputState.HasUnknownQuestion && !InputState.HasAdvice;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            SetExpectAnswer(true);
            EmitResponse("I don't know. What is the correct answer please?");
        }
    }
}