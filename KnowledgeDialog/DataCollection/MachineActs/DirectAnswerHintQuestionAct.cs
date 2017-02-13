using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class DirectAnswerHintQuestionAct : MachineActionBase
    {
        /// <summary>
        /// Presented question.
        /// </summary>
        private readonly ParsedUtterance _question;

        public DirectAnswerHintQuestionAct(ParsedUtterance question)
        {
            _question = question;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return string.Format("{0}", _question.OriginalSentence);
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation = new ActRepresentation("DirectAnswerHintQuestion");

            representation.AddParameter("question", _question.OriginalSentence);
            return representation;
        }
    }
}
