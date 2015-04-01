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

        private readonly Dictionary<string, HashSet<string>> _disabledEquivalencies = new Dictionary<string, HashSet<string>>();

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

        public IEnumerable<Tuple<T, string, double, string>> ScoredSubstitutionMap(string utterance)
        {
            var result = new List<Tuple<T, string, double, string>>();
            var parsedSentence = SentenceParser.Parse(utterance);

            foreach (var pair in Mapping)
            {
                var similarity = getSimilarity(pair.Key, parsedSentence);

                result.Add(Tuple.Create(pair.Value, similarity.Item1, similarity.Item2, pair.Key.OriginalSentence));
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

        public void DisableEquivalence(string pattern, string utterance)
        {
            disableEquivalence(pattern, utterance);
            disableEquivalence(utterance, pattern);
        }

        private void disableEquivalence(string utterance1, string utterance2)
        {
            var signature1 = getSignature(SentenceParser.Parse(utterance1));
            var signature2 = getSignature(SentenceParser.Parse(utterance2));

            HashSet<string> disabled;
            if (!_disabledEquivalencies.TryGetValue(signature1, out disabled))
                _disabledEquivalencies[signature1] = disabled = new HashSet<string>();

            disabled.Add(signature2);
        }

        private string getSignature(ParsedSentence sentence)
        {
            var builder = new StringBuilder();
            foreach (var word in sentence.Words)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                if (_graph.HasEvidence(word))
                    builder.Append("#node");
                else
                    builder.Append(word);
            }

            return builder.ToString();
        }

        private Tuple<string, double> getSimilarity(ParsedSentence pattern, ParsedSentence sentence)
        {
            if (pattern.OriginalSentence == sentence.OriginalSentence)
                //we have exact match
                return Tuple.Create<string, double>(null, 1.0);

            if (!canBeEquivalent(pattern, sentence))
            {
                return Tuple.Create<string, double>(null, 0);
            }

            var patternWords = new HashSet<string>(pattern.Words);
            var sentenceWords = new HashSet<string>(sentence.Words);

            var isPattern = patternWords.Remove("*");
            var union = patternWords.Union(sentenceWords).ToArray();
            var intersection = patternWords.Intersect(sentenceWords).ToArray();

            var nodeWords = union.Where(w => _graph.HasEvidence(w)).ToArray();
            var syntacticWords = union.Except(nodeWords).ToArray();

            string substitution = null;
            if (isPattern)
            {
                //TODO improve patterning
                var isStartPattern = patternWords.First() == "*";

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
                }

                substitution = isStartPattern ? minWord : maxWord;
                if (substitution == null)
                    //substitution doesn't fit
                    return Tuple.Create(substitution, 0.0);
            }

            var substitutionBonus = isPattern ? 1.0 : 0.0;

            var syntacticUnion = union.Intersect(syntacticWords).ToArray();
            var syntacticIntersection = intersection.Intersect(syntacticWords).ToArray();
            var syntacticSimilarity = 1.0 * (syntacticIntersection.Length + 1) / (syntacticUnion.Length + 1);

            var nodeUnion = union.Intersect(nodeWords).ToArray();
            var nodeIntersection = intersection.Intersect(nodeWords).ToArray();
            var nodeSimilarity = 1.0 * (nodeIntersection.Length + 1 + substitutionBonus) / (nodeUnion.Length + 1);

            var similarity = 0.9 * syntacticSimilarity + 0.1 * nodeSimilarity;

            return Tuple.Create(substitution, similarity);
        }

        private bool canBeEquivalent(ParsedSentence pattern, ParsedSentence sentence)
        {
            var sentenceSignature = getSignature(pattern);
            var patternSignature = getSignature(sentence);

            HashSet<string> disabled;
            return !(
                _disabledEquivalencies.TryGetValue(patternSignature, out disabled) &&
                disabled.Contains(sentenceSignature)
                );            
        }

        public void DesiredScore(object index, double score)
        {
            //throw new NotImplementedException();
        }
    }
}
