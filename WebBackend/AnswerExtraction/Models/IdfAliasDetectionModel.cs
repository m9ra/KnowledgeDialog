using KnowledgeDialog.Dialog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace WebBackend.AnswerExtraction.Models
{
    class IdfAliasDetectionModel
    {
        private readonly Dictionary<string, int> _NonEntityPhraseInverseCounts = new Dictionary<string, int>();

        public IEnumerable<KeyValuePair<string, int>> NonEntityPhrasesInverseCounts
        {
            get
            {
                return _NonEntityPhraseInverseCounts;
            }
        }

        public void Accept(LinkedUtterance utterance)
        {
            var phrases = getNonEntityPhrases(utterance).Distinct().ToArray();

            foreach (var phrase in phrases)
            {
                _NonEntityPhraseInverseCounts.TryGetValue(phrase, out var count);
                _NonEntityPhraseInverseCounts[phrase] = count + 1;
            }
        }

        private IEnumerable<string> getNonEntityPhrases(LinkedUtterance utterance)
        {
            var currentWords = new List<string>();
            var phrases = new List<string>();

            foreach (var part in utterance.Parts)
            {
                if (!part.Entities.Any())
                {
                    currentWords.Add(part.Token);
                    continue;
                }

                if (currentWords.Count > 0)
                    phrases.Add(string.Join(" ", currentWords));

                currentWords.Clear();
            }

            return phrases;
        }
    }
}
