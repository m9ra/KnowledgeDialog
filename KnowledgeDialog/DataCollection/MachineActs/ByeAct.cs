using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class ByeAct : MachineActionBase
    {
        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return "Thank you for your help, bye.";
        }

        /// <inheritdoc/>
        protected override string initializeDialogActRepresentation()
        {
            return "Bye()";
        }
    }
}
