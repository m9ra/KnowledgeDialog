using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.Acts;

namespace KnowledgeDialog.Dialog
{
    public abstract class ResponseBase
    {
        /// <summary>
        /// Gets semantic dialog act representation.
        /// </summary>
        /// <returns>The representation when overriden.</returns>
        public virtual string GetDialogActRepresentation()
        {
            return null;
        }

        /// <summary>
        /// Gets semantic dialog act json representation.
        /// </summary>
        /// <returns>The representation when overriden.</returns>
        public virtual ActRepresentation GetDialogAct()
        {
            return null;
        }
    }
}
