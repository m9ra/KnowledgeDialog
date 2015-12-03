using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Experiment
{
    class AnnotatedActionEntry
    {
        private readonly ActionEntry _entry;

        public readonly string CorrectAnswer;

        public int ActionIndex { get { return _entry.ActionIndex; } }

        public string Act { get { return _entry.Act; } }

        public string Text { get { return _entry.Text; } }

        public string Type { get { return _entry.Type; } }

        public bool IsReset { get { return _entry.IsReset; } }

        public DateTime Time { get { return _entry.Time; } }

        internal AnnotatedActionEntry(ActionEntry entry, string correctAnswer)
        {
            _entry = entry;
            CorrectAnswer = correctAnswer;
        }
    }
}
