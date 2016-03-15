using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class TooBriefExplanationAct : MachineActionBase
    {
        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return "I am sorry, but your explanation is too brief. Could you provide more details please?";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            return new ActRepresentation("TooBriefExplanation");            
        }
    }
}
