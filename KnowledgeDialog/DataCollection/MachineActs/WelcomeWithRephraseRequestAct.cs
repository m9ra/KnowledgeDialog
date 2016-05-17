using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class WelcomeWithRephraseRequestAct : MachineActionBase
    {
        /// <summary>
        /// Question presented with the weolcome.
        /// </summary>
        private readonly ParsedUtterance _question;

        internal WelcomeWithRephraseRequestAct(ParsedUtterance question)
        {
            _question = question;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return string.Format("Hello, I need help with this question: '{0}'. Can you put this question in a different way?", _question.OriginalSentence);
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation = new ActRepresentation("WelcomeWithRephraseRequest");

            representation.AddParameter("question", _question.OriginalSentence);
            return representation;
        }
    }
}
