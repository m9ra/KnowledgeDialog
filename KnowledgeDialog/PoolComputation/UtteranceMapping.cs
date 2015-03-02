using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class UtteranceMapping<T>
    {
        private readonly Dictionary<string, T> _mapping = new Dictionary<string, T>();

        private readonly ComposedGraph _graph;

        public UtteranceMapping(ComposedGraph graph)
        {
            _graph = graph;
        }

        public T BestMap(string utterance)
        {
            return ScoredMap(utterance).First().Item1;
        }

        public IEnumerable<Tuple<T, double>> ScoredMap(string utterance)
        {
            var result = new List<Tuple<T, double>>();
            foreach (var pair in _mapping)
            {
                var similarity = getSimilarity(pair.Key, utterance);

                result.Add(Tuple.Create(pair.Value,similarity));
            }

            result.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            return result;
        }

        public void SetMapping(string utterance, T data)
        {
            _mapping[utterance] = data;
        }

        private double getSimilarity(string utterance1, string utterance2)
        {
            if (utterance1 == utterance2)
                //we have exact match
                return 1;

            var words1 = new HashSet<string>(getWords(utterance1));
            var words2 = new HashSet<string>(getWords(utterance2));

            var intersection = words1.Intersect(words2).ToArray();
            var union = words1.Union(words2).ToArray();

            return 0.9 * intersection.Count() / union.Count();
        }

        private IEnumerable<string> getWords(string utterance)
        {
            var words = utterance.Split(' ');
            foreach (var word in words)
            {
                if (!_graph.HasEvidence(word))
                    yield return word;
            }
        }
    }
}
