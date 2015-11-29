using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class ContinueAct : MachineActionBase
    {
        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return "That's great, let tell it to me.";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            return new ActRepresentation("Continue");
        }
    }
}
