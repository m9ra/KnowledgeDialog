using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class InputPhrase : PointableBase
    {
        private readonly string[] _words;

        private InputPhrase(IEnumerable<string> words)
        {
            _words = words.ToArray();
        }

        internal InputPhrase ExpandBy(string word)
        {
            return new InputPhrase(_words.Concat(new[] { word }));
        }

        internal static InputPhrase FromWord(string word)
        {
            return new InputPhrase(new[] { word });
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
