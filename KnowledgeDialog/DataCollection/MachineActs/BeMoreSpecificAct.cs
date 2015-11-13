using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class BeMoreSpecificAct : MachineActionBase
    {
        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return "I'm not able to find your answer in my database. Could you be more specific please?";
        }

        /// <inheritdoc/>
        protected override string initializeDialogActRepresentation()
        {
            return "BeMoreSpecific()";
        }
    }
}
