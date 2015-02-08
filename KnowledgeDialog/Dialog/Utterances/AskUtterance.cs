using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.Dialog.Utterances
{
    class AskUtterance : UtteranceBase
    {
        internal readonly string Question;

        private AskUtterance(string question)
        {
            Question = question;
        }

        internal static AskUtterance TryParse(string utterance)
        {
            return new AskUtterance(utterance.TrimEnd('?'));
        }

        protected override ResponseBase handleManager(IDialogManager manager)
        {
            return manager.Ask(Question);           
        }
    }
}
