﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class UtteranceMapping<T> : IMappingProvider<T>
    {
        protected readonly Dictionary<ParsedUtterance, T> Mapping = new Dictionary<ParsedUtterance, T>();

        private readonly ComposedGraph _graph;

        private readonly Dictionary<string, HashSet<string>> _disabledEquivalencies = new Dictionary<string, HashSet<string>>();

        public UtteranceMapping(ComposedGraph graph)
        {
            _graph = graph;
        }

        public MappingControl<T> BestMap(ParsedUtterance utterance)
        {
            return FindMapping(utterance).FirstOrDefault();
        }

        internal IEnumerable<MappingControl<T>> FindMapping(ParsedUtterance parsedSentence)
        {
            var result = new List<MappingControl<T>>();

            foreach (var pair in Mapping)
            {
                var similarity = getSimilarity(pair.Key, parsedSentence);

                var control = new MappingControl<T>(similarity.Item1, similarity.Item2, this, pair.Value, pair.Key);
                result.Add(control);
            }

            result.Sort((a, b) => b.Score.CompareTo(a.Score));

            return result;
        }

        public void SetMapping(string utterance, T data)
        {
            var sentence = UtteranceParser.Parse(utterance);
            Mapping[sentence] = data;
        }

        public void DisableEquivalence(string pattern, string utterance)
        {
            disableEquivalence(pattern, utterance);
            disableEquivalence(utterance, pattern);
        }

        private void disableEquivalence(string utterance1, string utterance2)
        {
            var signature1 = getSignature(UtteranceParser.Parse(utterance1));
            var signature2 = getSignature(UtteranceParser.Parse(utterance2));

            HashSet<string> disabled;
            if (!_disabledEquivalencies.TryGetValue(signature1, out disabled))
                _disabledEquivalencies[signature1] = disabled = new HashSet<string>();

            disabled.Add(signature2);
        }

        private string getSignature(ParsedUtterance sentence)
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

        private Tuple<string, double> getSimilarity(ParsedUtterance pattern, ParsedUtterance sentence)
        {
            if (pattern.OriginalSentence == sentence.OriginalSentence)
                //we have exact match
                return Tuple.Create<string, double>(null, 1.0);

            if (!canBeEquivalent(pattern, sentence))
            {
                return Tuple.Create<string, double>(null, 0);
            }

            var comparer = StringComparer.InvariantCultureIgnoreCase;

            var patternWords = new HashSet<string>(pattern.Words, comparer);
            var sentenceWords = new HashSet<string>(sentence.Words, comparer);

            var isStartPattern = patternWords.FirstOrDefault() == "*";
            var isPattern = patternWords.Remove("*");
            var union = patternWords.Union(sentenceWords, comparer).ToArray();
            var intersection = patternWords.Intersect(sentenceWords, comparer).ToArray();

            var nodeWords = union.Where(w => _graph.HasEvidence(w)).ToArray();
            var syntacticWords = union.Except(nodeWords, comparer).ToArray();

            string substitution = null;
            if (isPattern)
            {
                //TODO improve patterning                
                var minIndex = int.MaxValue;
                var maxIndex = int.MinValue;
                string minWord = null, maxWord = null;
                foreach (var word in sentenceWords.Except(intersection, comparer))
                {
                    var index = sentence.OriginalSentence.IndexOf(word, StringComparison.InvariantCultureIgnoreCase);
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

            var syntacticUnion = union.Intersect(syntacticWords, comparer).ToArray();
            var syntacticIntersection = intersection.Intersect(syntacticWords, comparer).ToArray();
            var syntacticSimilarity = 1.0 * (syntacticIntersection.Length + 1) / (syntacticUnion.Length + 1);

            var nodeUnion = union.Intersect(nodeWords, comparer).ToArray();
            var nodeIntersection = intersection.Intersect(nodeWords, comparer).ToArray();
            var nodeSimilarity = 1.0 * (nodeIntersection.Length + 1 + substitutionBonus) / (nodeUnion.Length + 1);

            var similarity = 0.9 * syntacticSimilarity + 0.1 * nodeSimilarity;

            return Tuple.Create(substitution, similarity);
        }

        private bool canBeEquivalent(ParsedUtterance pattern, ParsedUtterance sentence)
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
            //TODO don't use this information for now
        }
    }
}
