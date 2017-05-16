using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public class ExplicitAdviceAct : AdviceAct
    {
        internal readonly ParsedUtterance Question;

        internal ExplicitAdviceAct(ParsedUtterance question, ParsedUtterance advice)
            :base(advice)
        {
            Question = question;
        }

        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "ExplicitAdvice(answer='" + Answer + "'; question='" + Question + "')";
        }

        /// <inheritdoc/>
        public override ActRepresentation GetDialogAct()
        {
            var act = new ActRepresentation("ExplicitAdvice");
            act.AddParameter("answer", Answer);
            act.AddParameter("question", Question);
            return act;
        }
    }
}
