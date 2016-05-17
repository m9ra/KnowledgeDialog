using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    public interface IInformativeFeedbackProvider
    {
        /// <summary>
        /// Determine whether the provider had encountered the informative feedback.
        /// </summary>
        bool HadInformativeInput { get; }

        /// <summary>
        /// Determine whether task based informative inputs can be completed.
        /// </summary>
        bool CanBeCompleted { get; }
    }
}
