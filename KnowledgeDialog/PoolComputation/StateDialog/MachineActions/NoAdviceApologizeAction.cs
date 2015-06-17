using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class NoAdviceApologizeAction : MachineActionBase
    {
        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return InputState.HasUnknownQuestion && !InputState.HasAdvice && InputState.HasNegation;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            RemoveUnknownQuestion();
            RemoveNegation();
            EmitResponse("I don't know correct answer. Give me another question please.");
        }
    }
}