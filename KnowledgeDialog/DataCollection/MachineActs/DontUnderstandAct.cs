using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class DontUnderstandAct : MachineActionBase
    {
        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return "I am sorry, but I could not understand you.";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            return new ActRepresentation("DontUnderstand");
        }
    }
}
