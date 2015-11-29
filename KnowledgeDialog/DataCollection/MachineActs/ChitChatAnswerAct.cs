using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class ChitChatAnswerAct : MachineActionBase
    {
        private readonly ChitChatDomain _domain;

        internal ChitChatAnswerAct(ChitChatDomain domain)
        {
            _domain = domain;
        }

        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            switch (_domain)
            {
                case ChitChatDomain.Welcome:
                    return "Can you ask me some question please?";

                case ChitChatDomain.Polite:
                case ChitChatDomain.Personal:
                    return "I can't talk about my personality, lets return to the question.";

                case ChitChatDomain.Rude:
                    return "I'm sorry for disappointing you, unfortunatelly we should return to the question.";
            }

            return "I'm sorry, but I don't understand";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation=new ActRepresentation("ChitChatAnswer");

            representation.AddParameter("domain", _domain);
            return representation;
        }
    }
}
