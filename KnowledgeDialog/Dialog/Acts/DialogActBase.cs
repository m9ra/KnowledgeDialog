using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public abstract class DialogActBase
    {
        abstract internal void Visit(IActVisitor visitor);

        /// <summary>
        /// Gets semantic dialog act json representation.
        /// </summary>
        /// <returns>The representation when overriden.</returns>
        public abstract ActRepresentation GetDialogAct();
    }
}
