using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.Dialog.Utterances
{
    public class AdviceUtterance : UtteranceBase
    {
        internal readonly string Question;

        internal readonly string Advice;

        private AdviceUtterance(string question, string answer)
        {
            Question = question.Trim();
            Advice = answer.Trim();
        }

        public static AdviceUtterance TryParse(string utterance)
        {
            if (!utterance.Contains(" is "))
                return null;

            var parts = utterance.Split(new[] { " is " }, 2, StringSplitOptions.RemoveEmptyEntries);

            return new AdviceUtterance(parts[0], parts[1]);
        }

        protected override ResponseBase handleManager(IDialogManager manager)
        {
            return manager.Advise(Question, Advice);
        }
    }
}
