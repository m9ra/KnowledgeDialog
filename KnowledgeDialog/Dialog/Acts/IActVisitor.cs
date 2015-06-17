using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    interface IActVisitor
    {
        void Visit(AdviceAct adviceAct);

        void Visit(AffirmAct confirmAct);

        void Visit(ThinkAct thinkAct);

        void Visit(NegateAct negateAct);

        void Visit(ExplicitAdviceAct explicitAdviceAct);

        void Visit(QuestionAct questionAct);

        void Visit(UnrecognizedAct unrecognizedAct);

        void Visit(ChitChatAct chitChatAct);
    }
}
