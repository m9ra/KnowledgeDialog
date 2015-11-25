using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected override string initializeDialogActRepresentation()
        {
            return "UnwantedRephraseDetected()";
        }
    }
}
