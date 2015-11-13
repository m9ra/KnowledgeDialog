using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    public abstract class ResponseBase
    {
        /// <summary>
        /// Initialization for semantic dialog act representation.
        /// </summary>
        /// <returns>The representation when overriden.</returns>
        public virtual string GetDialogActRepresentation()
        {
            return null;
        }
    }
}
