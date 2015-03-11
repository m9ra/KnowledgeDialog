using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    class ParsedSentence
    {
        public readonly IEnumerable<string> Words;

        public ParsedSentence(IEnumerable<string> words)
        {
            Words = words.ToArray();
        }
    }
}
