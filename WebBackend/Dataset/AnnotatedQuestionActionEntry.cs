using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Dataset
{
    class AnnotatedQuestionActionEntry
    {
        internal readonly ActionEntry Entry;

        public readonly string Annotation;

        public int ActionIndex { get { return Entry.ActionIndex; } }

        public string UserId { get { return Entry.UserId; } }

        public string Act { get { return Entry.Act; } }

        public string Text { get { return Entry.Text; } }

        public string Type { get { return Entry.Type; } }

        public bool IsReset { get { return Entry.IsReset; } }

        public DateTime Time { get { return Entry.Time; } }

        internal bool IsRegularTurn
        {
            get
            {
                return
                    Entry.Type == "T_utterance" ||
                    Entry.Type == "T_response"
                    ;
            }
        }

        public bool IsDialogStart
        {
            get
            {
                var isOpeningAct = Entry.Act != null && (
                    Entry.Act.StartsWith("WelcomeWithRephraseRequest(") ||
                    Entry.Act.StartsWith("RephraseQuestionPropose(")
                    );
                ;

                return isOpeningAct;
            }
        }

        internal AnnotatedQuestionActionEntry(ActionEntry entry, string annotation)
        {
            Entry = entry;
            Annotation = annotation;
        }

        internal string ParseQuestion()
        {
            //TODO this is bug in saving mechanism  

            string question;
            if (
                parseQuestion(out question, "WelcomeWithRephraseRequest(question=", ")##") ||
                parseQuestion(out question, "RephraseQuestionPropose(question=", ",at_least=")
            )
                return question;
            else
                throw new NotImplementedException("Question not parsed");
        }

        private bool parseQuestion(out string question, string prefix, string suffix)
        {
            question = null;

            var act = Entry.Act + "##";
            if (!act.StartsWith(prefix))
                return false;

            var startIndex = prefix.Length;
            var endIndex = act.IndexOf(suffix, startIndex);

            question = act.Substring(startIndex, endIndex - startIndex);
            return true;
        }
    }
}
