using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.QuestionAnswering;

namespace KnowledgeDialog.Dialog
{
    /// <summary>
    /// Manager for question answering.
    /// </summary>
    public interface IQuestionAnswerManager
    {
        void InitializeNewDialog();

        QuestionAnswerReceiveResult ReceiveQuestionPart(IEnumerable<TurnLog> questionTurns);

        QuestionAnswerReceiveResult ReceiveExplanationPart(IEnumerable<TurnLog> explanationTurns);

        QuestionAnswerReceiveResult ReceiveAnswerPart(IEnumerable<TurnLog> answerTurns);
    }

    public class QuestionAnswerReceiveResult
    {
        /// <summary>
        /// Entities for answer, if empty, there is no result available.
        /// </summary>
        public readonly IEnumerable<string> AnswerEntities;

        /// <summary>
        /// Determine whether system doesn't know answer for the question. In that case <see cref="AnswerEntities"/> are irrelevant.
        /// </summary>
        public readonly bool IsKnown;

        /// <summary>
        /// Determine whether further hint is needed by the system.
        /// </summary>
        public readonly bool RequiresHint;
    }

}
