using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public class UnrecognizedAct : DialogActBase
    {
        public readonly ParsedUtterance Utterance;

        public UnrecognizedAct(ParsedUtterance utterance)
        {
            Utterance = utterance;
        }

        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "Unrecognized(utterance='" + Utterance + "')";
        }

        /// <inheritdoc/>
        public override ActRepresentation GetDialogAct()
        {
            var unrecognized = new ActRepresentation("Unrecognized");
            unrecognized.AddParameter("utterance", Utterance);

            return unrecognized;
        }
    }
}
