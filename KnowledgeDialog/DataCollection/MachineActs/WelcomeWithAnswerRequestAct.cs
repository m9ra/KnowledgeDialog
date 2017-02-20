using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class WelcomeWithAnswerRequestAct : MachineActionBase
    {
        /// <summary>
        /// Question presented with the weolcome.
        /// </summary>
        private readonly ParsedUtterance _question;

        public WelcomeWithAnswerRequestAct(ParsedUtterance question)
        {
            _question = question;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return string.Format("Hello, I need help with this question: '{0}'. Can you give me an answer?", _question.OriginalSentence);
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation = new ActRepresentation("WelcomeWithAnswerRequest");

            representation.AddParameter("question", _question.OriginalSentence);
            return representation;
        }
    }
}
