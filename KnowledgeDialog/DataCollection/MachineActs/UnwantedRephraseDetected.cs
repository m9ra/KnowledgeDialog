using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class UnwantedRephraseDetected : MachineActionBase
    {
        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return "I think that you just rephrased the question. Could you rather give me an explanation of the question?";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()        
        {
            return new ActRepresentation("UnwantedRephraseDetected");            
        }
    }
}
