using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace WebBackend.Dataset
{
    public class WordStats
    {
        Dictionary<string, int> _wordCounts = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public WordStats(IEnumerable<string> utterances)
        {
            foreach (var utterance in utterances)
            {
                var parsed = UtteranceParser.Parse(utterance);
                foreach (var word in parsed.Words)
                {
                    int count;
                    _wordCounts.TryGetValue(word, out count);
                    ++count;
                    _wordCounts[word] = count;
                }
            }
        }

        public IEnumerable<Tuple<string, int>> WordCounts
        {
            get
            {
                return _wordCounts.Select(p => Tuple.Create(p.Key, p.Value)).ToArray();
            }
        }

        public IEnumerable<Tuple<string, int>> InformativeWordCounts
        {
            get
            {
                return WordCounts.Where(t => UtteranceParser.IsInformativeWord(t.Item1)).ToArray();
            }
        }

        public IEnumerable<Tuple<string, int>> TopInformativeWordCounts { get { return InformativeWordCounts.OrderByDescending(c => c.Item2).Take(20).ToArray(); } }

        public IEnumerable<Tuple<string, int>> TopWordCounts { get { return TopWordCounts.OrderByDescending(c => c.Item2).Take(20).ToArray(); } }

        public int WordCount { get { return _wordCounts.Values.Sum(); } }
    }
}
