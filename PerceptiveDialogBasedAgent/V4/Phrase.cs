using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Phrase : PointableInstance
    {
        private readonly string[] _words;

        internal int WordCount => _words.Length;

        private Phrase(IEnumerable<string> words)
            : base(null)
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
            utterance = " " + utterance + " ";
            utterance = utterance.Replace(",", " ").Replace(".", " ").Replace("?", " ").Replace("!", " ").Replace("  ", " ").Replace("  ", " ");
            utterance = utterance.Replace("'nt ", " not ");
            utterance = utterance.Replace("'s ", " is ");//TODO 's can also expand to has
            utterance = utterance.Replace("'re ", " are ");
            return utterance.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

        internal override IEnumerable<PointableInstance> GetPropertyValue(Concept2 property)
        {
            return null;
        }
    }
}
