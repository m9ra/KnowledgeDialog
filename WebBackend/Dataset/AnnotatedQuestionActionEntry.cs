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

        internal readonly string Annotation;

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

        public bool IsDialogStart { get { throw new NotImplementedException(); } }

        internal AnnotatedQuestionActionEntry(ActionEntry entry, string annotation)
        {
            Entry = entry;
            Annotation = annotation;
        }

        internal string ParseQuestion()
        {
            throw new NotImplementedException();
        }
    }
}
