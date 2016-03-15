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
                    return "Nice to meet you! Let’s return to the question.";

                case ChitChatDomain.Polite:
                case ChitChatDomain.Personal:
                    return "I am sorry but I cannot talk about my personality. Let us return to the question.";

                case ChitChatDomain.Rude:
                    return "I am sorry for disappointing you, but unfortunately we should return to the question.";
            }

            return "I am sorry, but I could not understand you.";
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
