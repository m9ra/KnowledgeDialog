using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.Dialog.Utterances
{
    abstract class UtteranceBase
    {
        protected abstract ResponseBase handleManager(IDialogManager manager);

        internal ResponseBase HandleManager(IDialogManager manager)
        {
            return handleManager(manager);
        }
    }
}
