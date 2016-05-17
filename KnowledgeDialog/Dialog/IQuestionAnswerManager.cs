using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
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
        /// Confidence of answer.
        /// </summary>
        public readonly double Confidence;

        /// <summary>
        /// Entities for answer, if empty, there is no result available.
        /// </summary>
        public readonly IEnumerable<string> AnswerEntities;

        /// <summary>
        /// Determine whether system know answer for the question. In that case <see cref="AnswerEntities"/> are irrelevant.
        /// </summary>
        public readonly bool IsKnown;

        /// <summary>
        /// Determine whether further hint is needed by the system.
        /// </summary>
        public readonly bool RequiresHint;

        private QuestionAnswerReceiveResult(bool requiresHint, bool isKnown, IEnumerable<string> answerEntities, double confidence)
        {
            RequiresHint = requiresHint;
            IsKnown = isKnown;
            AnswerEntities = answerEntities.ToArray();
            Confidence = confidence;
        }

        /// <summary>
        /// Creates receive result which reports the answer.
        /// </summary>
        /// <param name="answer">The answer.</param>
        /// <returns>The created result.</returns>
        internal static QuestionAnswerReceiveResult From(Ranked<IEnumerable<NodeReference>> answer)
        {
            var entities = answer.Value.Select(s => s.Data);
            return new QuestionAnswerReceiveResult(false, true, entities, answer.Rank);
        }

        /// <summary>
        /// Creates receive result which indicates need for another hint.
        /// </summary>
        /// <param name="confidence">Confidence that hint is needed.</param>
        /// <returns>The created result.</returns>
        internal static QuestionAnswerReceiveResult HintNeeded(double confidence)
        {
            return new QuestionAnswerReceiveResult(true, false, null, confidence);
        }
    }
}
