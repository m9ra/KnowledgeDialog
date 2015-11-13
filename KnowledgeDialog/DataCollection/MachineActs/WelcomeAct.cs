using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class WelcomeAct : MachineActionBase
    {
        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return "Hello, how can I help you?";
        }

        /// <inheritdoc/>
        protected override string initializeDialogActRepresentation()
        {
            return "Welcome()";
        }
    }
}
