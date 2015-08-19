using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    public class ParsedUtterance
    {
        public IEnumerable<string> Words { get { return _words; } }

        public readonly string OriginalSentence;

        private readonly string[] _words;

        internal ParsedUtterance(string originalSentence, IEnumerable<string> words)
        {
            OriginalSentence = originalSentence;
            _words = words.ToArray();
        }


        static internal ParsedUtterance From(IEnumerable<string> words)
        {
            return new ParsedUtterance(string.Join(" ", words), words);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var accumulator = 0;
            for (var i = 0; i < _words.Length; ++i)
            {
                accumulator += _words[i].Length;
            }

            return accumulator;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as ParsedUtterance;
            if (o == null)
                return false;

            if (_words.Length != o._words.Length)
                return false;

            for (var i = 0; i < _words.Length; ++i)
            {
                if (_words[i] != o._words[i])
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Join(" ", Words);
        }
    }
}
