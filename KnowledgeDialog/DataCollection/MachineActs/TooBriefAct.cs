using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class TooBriefAct : MachineActionBase
    {
        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return "Your explanation is too brief. Can you provide more details please?";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            return new ActRepresentation("TooBrief");            
        }
    }
}
