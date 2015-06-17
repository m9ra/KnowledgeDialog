using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public enum ChitChatDomain { Rude, Polite, Personal }

    class ChitChatAct : DialogActBase
    {
        public readonly ChitChatDomain Domain;

        public ChitChatAct(ChitChatDomain domain)
        {
            Domain = domain;
        }

        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "ChitChat(domain='" + Domain + "')";
        }
    }
}
