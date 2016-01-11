using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SimpleQuestions
{
    class QuestionFileReader
    {
        private readonly SimpleQuestionEntry[] _entries;

        internal QuestionFileReader(string file)
        {
            var entries = new List<SimpleQuestionEntry>();
            var questionStreamReader =
   new StreamReader(file);
            string line;
            while ((line = questionStreamReader.ReadLine()) != null)
            {
                entries.Add(new SimpleQuestionEntry(line));
            }

            _entries = entries.ToArray();
        }

        internal IEnumerable<SimpleQuestionEntry> GetEntries()
        {
            return _entries;
        }
    }
}
