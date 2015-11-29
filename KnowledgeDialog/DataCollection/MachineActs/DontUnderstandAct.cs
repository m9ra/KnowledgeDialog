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
            return "I'm sorry, but I can't recognize a question in your utterance. Can you ask me by different words?";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            return new ActRepresentation("DontUnderstand");
        }
    }
}
