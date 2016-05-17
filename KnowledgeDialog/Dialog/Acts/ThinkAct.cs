using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public class ThinkAct : DialogActBase
    {
        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public override ActRepresentation GetDialogAct()
        {
            return new ActRepresentation("Think");
        }
    }
}
