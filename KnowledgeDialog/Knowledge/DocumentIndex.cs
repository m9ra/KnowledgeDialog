using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    public class DocumentIndex
    {
        private readonly Dictionary<string, int> _documentOccurences = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _totalOccurences = new Dictionary<string, int>();

        public void RegisterDocument(IEnumerable<string[]> document)
        {
            var documentWords = new HashSet<string>();
            foreach (var sentence in document)
            {
                foreach (var word in sentence)
                {
                    documentWords.Add(word);
                    _totalOccurences.TryGetValue(word, out var occurenceCount);

                    _totalOccurences[word] = occurenceCount + 1;
                }
            }

            foreach(var documentWord in documentWords)
            {
                _documentOccurences.TryGetValue(documentWord, out var documentCount);
                _documentOccurences[documentWord] = documentCount + 1;
            }
        }

        public double TfIdf(string word)
        {
            _totalOccurences.TryGetValue(word, out var totalCount);
            _documentOccurences.TryGetValue(word, out var documentCount);

            return 1.0 * totalCount / documentCount;
        }

        public double TotalOccurences(string word)
        {
            _totalOccurences.TryGetValue(word, out var totalCount);

            return totalCount;
        }
    }
}
