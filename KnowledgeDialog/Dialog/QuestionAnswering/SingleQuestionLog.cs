using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.QuestionAnswering
{
    /// <summary>
    /// Represents a dialog about single question from QA point of view.
    /// </summary>
    public class SingleQuestionLog
    {
        public bool HasAnnotation { get { return LastQuestionTurnAnnotation != null; } }

        public readonly IEnumerable<TurnLog> QuestionTurns;

        public readonly IEnumerable<TurnLog> ExplanationTurns;

        public readonly IEnumerable<TurnLog> AnswerTurns;

        public readonly IEnumerable<string> LastQuestionTurnAnnotation;

        public SingleQuestionLog(IEnumerable<TurnLog> questionTurns, IEnumerable<TurnLog> explanationTurns, IEnumerable<TurnLog> answerTurns, IEnumerable<string> lastQuestionTurnAnnotation)
        {
            QuestionTurns = questionTurns.ToArray();
            ExplanationTurns = explanationTurns.ToArray();
            AnswerTurns = answerTurns.ToArray();
            LastQuestionTurnAnnotation = lastQuestionTurnAnnotation.ToArray();
        }
    }
}
