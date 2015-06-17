using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class WelcomeAction : MachineActionBase
    {
        /// </inheritdoc>
        protected override bool CouldApply()
        {
            //we would not like to welcome user multiple times
            return !InputState.IsUserWelcomed;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            EmitResponse("Hello, how can I help you?");
            SetWelcomedFlag(true);
        }
    }
}
