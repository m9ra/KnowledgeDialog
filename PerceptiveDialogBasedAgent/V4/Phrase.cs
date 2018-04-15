using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Phrase : PointableBase
    {
        private readonly string[] _words;

        private Phrase(IEnumerable<string> words)
        {
            _words = words.ToArray();
        }

        internal Phrase ExpandBy(string word)
        {
            return new Phrase(_words.Concat(new[] { word }));
        }

        internal static Phrase FromWord(string word)
        {
            return new Phrase(new[] { word });
        }

        internal static Phrase FromUtterance(string utterance)
        {
            return new Phrase(AsWords(utterance));
        }

        internal static string[] AsWords(string utterance)
        {
            return utterance.Split(' ');
        }

        internal override string ToPrintable()
        {
            return string.Join(" ", _words);
        }

        /// </inheritdoc>
        public override string ToString()
        {
            return string.Join(" ", _words);
        }
    }
}
