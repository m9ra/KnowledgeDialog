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

        /// </inheritdoc>
        public override bool Equals(object obj)
        {
            var o = obj as DialogActBase;
            if (o == null)
                return false;

            var repr = GetDialogAct();
            var oRerp = o.GetDialogAct();

            return repr.ToFunctionalRepresentation().Equals(oRerp.ToFunctionalRepresentation());
        }

        /// </inheritdoc>
        public override int GetHashCode()
        {
            return GetDialogAct().ToFunctionalRepresentation().GetHashCode();
        }
    }
}
