using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Dataset
{
    class DialogTurn
    {
        public readonly int TurnIndex;

        public readonly string OutputTranscript;

        public readonly string InputChat;

        internal DialogTurn(int turnIndex, string outputTranscript, string inputChat)
        {
            TurnIndex = turnIndex;
            OutputTranscript = outputTranscript;
            InputChat = inputChat;
        }
    }

    class QuestionDialog
    {
        internal readonly string AnswerMid;
        internal readonly bool HasCorrectAnswer;
        internal readonly string Question;
        internal readonly IEnumerable<DialogTurn> AnswerTurns;
        internal readonly IEnumerable<DialogTurn> ExplanationTurns;

        internal QuestionDialog(string answerMid, string annotation, string question,IEnumerable<DialogTurn> explanationTurns, IEnumerable<DialogTurn> answerTurns)
        {
            AnswerMid = answerMid;
            HasCorrectAnswer = annotation == "correct_answer";
            Question = question;
            ExplanationTurns = explanationTurns.ToArray();
            AnswerTurns = answerTurns.ToArray();
        }
    }
}
