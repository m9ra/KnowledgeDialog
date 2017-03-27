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

        Dictionary<string, double> _wordTfIdf = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);

        public IEnumerable<Tuple<string, int, double>> WordCounts
        {
            get
            {
                return _wordCounts.Select(p => Tuple.Create(p.Key, p.Value, getTfIdf(p.Key))).ToArray();
            }
        }

        public IEnumerable<Tuple<string, int, double>> InformativeWordCounts
        {
            get
            {
                return WordCounts.Where(t => UtteranceParser.IsInformativeWord(t.Item1)).ToArray();
            }
        }

        public IEnumerable<Tuple<string, int, double>> TopInformativeWordCounts { get { return InformativeWordCounts.OrderByDescending(c => c.Item3).ThenByDescending(c => c.Item2).Take(20).ToArray(); } }

        public IEnumerable<Tuple<string, int, double>> TopWordCounts
        {
            get
            {
                return
TopWordCounts.OrderByDescending(c => c.Item2).Take(20).ToArray();
            }
        }

        public int WordCount { get { return _wordCounts.Values.Sum(); } }

        public WordStats(IEnumerable<string> utterances)
        {
            initializeWordCounts(utterances);
        }

        public WordStats(IEnumerable<string> utterances, IEnumerable<IEnumerable<string>> allDocumentUtterances)
        {
            initializeWordCounts(utterances);

            var wordlist = allDocumentUtterances.SelectMany(d => d.SelectMany(u => getUtteranceWords(u))).Distinct().ToList();

            var documentIndex = allDocumentUtterances.Select(d => new HashSet<string>(d.SelectMany(u => getUtteranceWords(u)))).ToArray();
            foreach (var word in _wordCounts.Keys)
            {
                var documentOccurenceCount = 0;
                foreach (var document in documentIndex)
                {
                    if (document.Contains(word))
                        documentOccurenceCount += 1;
                }

                var count = _wordCounts[word];
                _wordTfIdf[word] = 1.0 * count / documentOccurenceCount;
            }
        }

        private void initializeWordCounts(IEnumerable<string> utterances)
        {
            foreach (var utterance in utterances)
            {
                var words = getUtteranceWords(utterance);
                foreach (var word in words)
                {
                    int count;
                    _wordCounts.TryGetValue(word, out count);
                    ++count;
                    _wordCounts[word] = count;
                }
            }
        }

        private string[] getUtteranceWords(string utterance)
        {
            var parsed = UtteranceParser.Parse(utterance);
            return parsed.Words.Distinct().ToArray();
        }

        private double getTfIdf(string word)
        {
            double value;
            _wordTfIdf.TryGetValue(word, out value);
            return value;
        }
    }
}
