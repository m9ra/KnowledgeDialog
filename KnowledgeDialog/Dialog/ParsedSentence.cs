using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    class ParsedSentence
    {
        public IEnumerable<string> Words { get { return _words; } }
        public readonly string OriginalSentence;

        private readonly string[] _words;

        public ParsedSentence(string originalSentence,IEnumerable<string> words)
        {
            OriginalSentence = originalSentence;
            _words = words.ToArray();
        }

        public override int GetHashCode()
        {
            var accumulator = 0;
            for (var i = 0; i < _words.Length; ++i)
            {
                accumulator += _words[i].Length;
            }

            return accumulator;
        }

        public override bool Equals(object obj)
        {
            var o = obj as ParsedSentence;
            if (o == null)
                return false;

            if (_words.Length != o._words.Length)
                return false;

            for (var i = 0; i < _words.Length; ++i) {
                if (_words[i] != o._words[i])
                    return false;
            }

            return true;
        }
    }
}
