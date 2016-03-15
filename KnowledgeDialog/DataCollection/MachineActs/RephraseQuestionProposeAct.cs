using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class RephraseQuestionProposeAct : MachineActionBase
    {
        /// <summary>
        /// Question presented with the weolcome.
        /// </summary>
        private readonly ParsedUtterance _question;

        /// <summary>
        /// Determine whether the rephrase is articuled to at least act.
        /// </summary>
        private readonly bool _isAtLeast;

        internal RephraseQuestionProposeAct(ParsedUtterance question, bool isAtLeast)
        {
            _question = question;
            _isAtLeast = isAtLeast;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            var pattern = _isAtLeast ?
                "Ok, could you please rephrase this next question: '{0}'?" :
                "Thanks! Could you please rephrase this next question: '{0}'?";

            return string.Format(pattern, _question.OriginalSentence);
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation = new ActRepresentation("RephraseQuestionPropose");

            representation.AddParameter("question", _question.OriginalSentence);
            representation.AddParameter("at_least", _isAtLeast);
            return representation;
        }
    }
}
