using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public class AdviceAct : DialogActBase
    {
        public readonly ParsedUtterance Answer;

        public AdviceAct(ParsedUtterance advice)
        {
            Answer = advice;
        }

        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "Advice(answer='" + Answer + "')";
        }

        /// <inheritdoc/>
        public override ActRepresentation GetDialogAct()
        {
            var act = new ActRepresentation("Advice");
            act.AddParameter("answer", Answer);

            return act;
        }
    }
}
