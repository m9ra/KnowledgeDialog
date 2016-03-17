using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Dataset
{
    class AnnotatedQuestionDialog
    {
        internal DateTime DialogEnd { get { return AnswerTurns.Last().Entry.Time; } }

        internal readonly string Question;

        internal readonly string AnswerId;

        internal readonly string Annotation;

        internal readonly IEnumerable<AnnotatedQuestionActionEntry> ExplanationTurns;

        internal readonly IEnumerable<AnnotatedQuestionActionEntry> AnswerTurns;

        internal AnnotatedQuestionDialog(string question, string answerId, string annotation, IEnumerable<AnnotatedQuestionActionEntry> explanationTurns, IEnumerable<AnnotatedQuestionActionEntry> answerTurns)
        {
            Question = question;
            AnswerId = answerId;
            Annotation = annotation;
            ExplanationTurns = explanationTurns.ToArray();
            AnswerTurns = answerTurns.ToArray();
        }
    }
}
