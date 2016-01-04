using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.QuestionAnswering;

namespace WebBackend.Dataset
{
    enum EvaluationCategory { Question=0, Explanation, Answer }

    /// <summary>
    /// Evaluates <see cref="IQuestionAnswerManager"/> on dataset.
    /// </summary>
    class Evaluator
    {
        private readonly int[] _correctPoints = new int[Enum.GetNames(typeof(EvaluationCategory)).Length];

        private readonly int[] _wrongPoints = new int[Enum.GetNames(typeof(EvaluationCategory)).Length];

        internal Evaluator(string dataset, IQuestionAnswerManager manager)
        {
            var reader = new DatasetReader(dataset);
            foreach (var dialog in reader.ReadDialogs())
            {
                manager.InitializeNewDialog();
                var hasAnswer = evaluate(manager.ReceiveQuestionPart, EvaluationCategory.Question, dialog.AnswerTurns, dialog) &&
                    evaluate(manager.ReceiveExplanationPart, EvaluationCategory.Explanation, dialog.ExplanationTurns, dialog) &&
                    evaluate(manager.ReceiveAnswerPart, EvaluationCategory.Question, dialog.AnswerTurns, dialog);
            }
        }

        /// <summary>
        /// Evaluates answer of given category.
        /// </summary>
        /// <param name="category">Category of evaluated part of dialog.</param>
        /// <param name="dialog">Dialog which part is evaluated.</param>
        /// <param name="answer">The evaluated answer.</param>
        private void evaluateAnswer(EvaluationCategory category, SingleQuestionLog dialog, IEnumerable<string> answer)
        {
            if (!dialog.HasAnnotation)
                //nothing to evaluate
                return;

            var isAnswerCorrect = Enumerable.SequenceEqual(dialog.LastQuestionTurnAnnotation.OrderBy(t => t), answer.OrderBy(t => t));

            var categoryIndex = (int)category;
            if (isAnswerCorrect)
                _correctPoints[categoryIndex] += 1;
            else
                _wrongPoints[categoryIndex] += 1;
        }

        /// <summary>
        /// Evaluates answer of given category.
        /// </summary>
        /// <param name="evaluatedTurns">Handler which is process given turns.</param>
        /// <param name="category">Category of evaluated part of dialog.</param>
        /// <param name="dialog">Dialog which part is evaluated.</param>
        /// <param name="answer">The evaluated answer.</param>
        private bool evaluate(Func<IEnumerable<TurnLog>, QuestionAnswerReceiveResult> handler, EvaluationCategory category, IEnumerable<TurnLog> evaluatedTurns, SingleQuestionLog dialog)
        {
            var answer = handler(dialog.QuestionTurns);
            if (!answer.IsKnown)
                return false;

            evaluateAnswer(category, dialog, answer.AnswerEntities);
            return true;
        }
    }
}
