using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class IncompleteByeAct : MachineActionBase
    {
        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return "Ok. Thank you anyway. Bye.";
        }

        /// <inheritdoc/>
        protected override string initializeDialogActRepresentation()
        {
            return "IncompleteBye()";
        }
    }
}
