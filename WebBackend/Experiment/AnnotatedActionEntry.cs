using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Experiment
{
    class AnnotatedActionEntry
    {
        internal readonly ActionEntry Entry;

        public readonly string CorrectAnswer;

        public int ActionIndex { get { return Entry.ActionIndex; } }

        public string Act { get { return Entry.Act; } }

        public string Text { get { return Entry.Text; } }

        public string Type { get { return Entry.Type; } }

        public bool IsReset { get { return Entry.IsReset; } }

        public DateTime Time { get { return Entry.Time; } }

        internal AnnotatedActionEntry(ActionEntry entry, string correctAnswer)
        {
            Entry = entry;
            CorrectAnswer = correctAnswer;
        }
    }
}
