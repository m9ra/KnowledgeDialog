using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.Dialog.Utterances
{
    class NoUtterance : UtteranceBase
    {
        internal static NoUtterance TryParse(string utterance)
        {
            if (!utterance.StartsWith("no"))
                return null;

            return new NoUtterance();
        }

        protected override ResponseBase handleManager(IDialogManager manager)
        {
            return manager.Negate();
        }
    }

}
