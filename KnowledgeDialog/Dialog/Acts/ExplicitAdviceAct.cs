using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public class ExplicitAdviceAct : DialogActBase
    {
        internal readonly ParsedUtterance Question;

        internal readonly ParsedUtterance Answer;

        internal ExplicitAdviceAct(ParsedUtterance question, ParsedUtterance advice)
        {
            Question = question;
            Answer = advice;
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
