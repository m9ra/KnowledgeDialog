using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class ByeAct : MachineActionBase
    {
        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return "Thank you for your help, goodbye.";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            return new ActRepresentation("Bye");
        }
    }
}
