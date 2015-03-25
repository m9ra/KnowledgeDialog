using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class UtteranceMapping<T> : IMappingProvider
    {
        protected readonly Dictionary<ParsedSentence, T> Mapping = new Dictionary<ParsedSentence, T>();

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
            var parsedSentence = SentenceParser.Parse(utterance);

            foreach (var pair in Mapping)
            {
                var similarity = getSimilarity(pair.Key, parsedSentence);

                result.Add(Tuple.Create(pair.Value, similarity.Item2));
            }

            result.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            return result;
        }

        public IEnumerable<Tuple<T, string, double>> ScoredSubstitutionMap(string utterance)
        {
            var result = new List<Tuple<T, string, double>>();
            var parsedSentence = SentenceParser.Parse(utterance);

            foreach (var pair in Mapping)
            {
                var similarity = getSimilarity(pair.Key, parsedSentence);

                result.Add(Tuple.Create(pair.Value, similarity.Item1, similarity.Item2));
            }

            result.Sort((a, b) => b.Item3.CompareTo(a.Item3));

            return result;
        }

        internal IEnumerable<Tuple<T, MappingControl>> ControlledMap(string utterance)
        {
            var result = new List<Tuple<T, MappingControl>>();
            var parsedSentence = SentenceParser.Parse(utterance);

            foreach (var pair in Mapping)
            {
                var similarity = getSimilarity(pair.Key, parsedSentence);

                var control = new MappingControl(similarity.Item1, similarity.Item2, null, this);
                result.Add(Tuple.Create(pair.Value, control));
            }

            result.Sort((a, b) => b.Item2.Score.CompareTo(a.Item2.Score));

            return result;
        }

        public void SetMapping(string utterance, T data)
        {
            var sentence = SentenceParser.Parse(utterance);
            Mapping[sentence] = data;
        }

        private Tuple<string, double> getSimilarity(ParsedSentence pattern, ParsedSentence sentence)
        {
            if (pattern.OriginalSentence == sentence.OriginalSentence)
                //we have exact match
                return Tuple.Create<string, double>(null, 1.0);

            var patternWords = new HashSet<string>(pattern.Words);
            var sentenceWords = new HashSet<string>(sentence.Words);

            var isPattern = patternWords.Remove("*");
            var intersection = patternWords.Intersect(sentenceWords).ToArray();
            var intersectionCount = intersection.Length;

            string substitution = null;
            if (isPattern)
            {
                //TODO improve patterning
                var isStartPattern = patternWords.First() == "*";
                ++intersectionCount;

                var minIndex = int.MaxValue;
                var maxIndex = int.MinValue;
                string minWord = null, maxWord = null;
                foreach (var word in sentenceWords.Except(intersection))
                {
                    var index = sentence.OriginalSentence.IndexOf(word);
                    if (index < minIndex)
                    {
                        minIndex = index;
                        minWord = word;
                    }

                    if (index > maxIndex)
                    {
                        maxIndex = index;
                        maxWord = word;
                    }

                    substitution = isStartPattern ? minWord : maxWord;
                    if (substitution == null)
                        //substitution doesn't fit
                        return Tuple.Create(substitution, 0.0);
                }
            }

            var union = patternWords.Union(sentenceWords).ToArray();
            var confidence = 0.9 * intersectionCount / union.Count();
            return Tuple.Create(substitution, confidence);
        }

        public void DesiredScore(object index, double score)
        {
            //throw new NotImplementedException();
        }
    }
}
