using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public class QuestionAct : DialogActBase
    {
        public readonly ParsedUtterance Question;

        internal QuestionAct(ParsedUtterance question)
        {
            Question = question;
        }

        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "Question(question='" + Question + "')";
        }

        /// <inheritdoc/>
        public override ActRepresentation GetDialogAct()
        {
            var act = new ActRepresentation("Question");

            act.AddParameter("question", Question);
            return act;
        }
    }
}
