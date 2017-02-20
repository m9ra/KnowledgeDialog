using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class WelcomeAct : MachineActionBase
    {
        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return "Hello, how can I help you?";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            return new ActRepresentation("Welcome");
        }
    }
}
