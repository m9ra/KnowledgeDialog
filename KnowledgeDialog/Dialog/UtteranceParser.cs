using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace KnowledgeDialog.Dialog
{
    public static class UtteranceParser
    {
        internal static readonly int MaxLookahead = 5;

        internal static readonly double IsEntityThreshold = 2.5;

        private static readonly HashSet<string> NonEntityWords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "is", "yes", "no" };

        private static readonly Regex _inputSanitizer = new Regex("[,.?<>;'\"-]", RegexOptions.Compiled);

        private static readonly Regex _spaceSanitizer = new Regex(@"[ ]{2,}", RegexOptions.Compiled);

        private static readonly Dictionary<string, List<string>> _indexedEntities = new Dictionary<string, List<string>>();

        private static readonly DoubleMetaphone _metaphone = new DoubleMetaphone();

        internal static void RegisterEntity(string entity)
        {
            if (!entity.Contains(' '))
                //only multi word entities has to be known by parser
                return;

            var indexCode = getCode(entity);

            List<string> entities;
            if (!_indexedEntities.TryGetValue(indexCode, out entities))
                _indexedEntities[indexCode] = entities = new List<string>();

            if (entities.Contains(entity))
                //nothing to do
                return;

            entities.Add(entity);
        }

        public static ParsedExpression Parse(string sentence)
        {
            sentence = _inputSanitizer.Replace(sentence, " ");
            sentence = _spaceSanitizer.Replace(sentence, " ").Trim();
            return parseExpression(sentence);
        }

        private static ParsedExpression parseExpression(string sanitizedSentence)
        {
            var singleWords = sanitizedSentence.Split(' ');
            var parsedWords = new List<string>();
            for (var currentWordIndex = 0; currentWordIndex < singleWords.Length; ++currentWordIndex)
            {
                var currentWord = singleWords[currentWordIndex];

                //try to find named entity in the lookahead
                var bestEntity = currentWord;
                var bestIndex = currentWordIndex;
                var currentEntity = currentWord;
                for (var lookaheadOffset = 1; lookaheadOffset < MaxLookahead && currentWordIndex + lookaheadOffset < singleWords.Length; ++lookaheadOffset)
                {
                    var lookAheadIndex = currentWordIndex + lookaheadOffset;
                    var lookaheadWord = singleWords[lookAheadIndex];

                    if (NonEntityWords.Contains(lookaheadWord) || NonEntityWords.Contains(currentWord))
                        //this word cannot be part of an entity
                        break;

                    //StringBuilder won't be better here - we need to build all the strings
                    currentEntity += " " + lookaheadWord;
                    var entityDistance = getBestEntityMatch(currentEntity);
                    if (entityDistance.Item2 < IsEntityThreshold)
                    {
                        //take the longes entity in the lookahead
                        bestEntity = entityDistance.Item1;
                        bestIndex = lookAheadIndex;
                    }
                }

                //consider lookahead in the loop
                currentWordIndex = bestIndex;
                parsedWords.Add(bestEntity);
            }

            return new ParsedExpression(sanitizedSentence, parsedWords);
        }



        private static Tuple<string, double> getBestEntityMatch(string ngram)
        {
            var code = getCode(ngram);

            List<string> entities;
            if (!_indexedEntities.TryGetValue(code, out entities))
                return Tuple.Create<string, double>(null, double.MaxValue);

            var minDistance = double.MaxValue;
            string candidate = null;
            foreach (var entity in entities)
            {
                var distance = levenshtein(ngram, entity);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    candidate = entity;
                }
            }

            return Tuple.Create(candidate, minDistance);
        }

        private static Int32 levenshtein(String a, String b)
        {
            a = a.ToLower();
            b = b.ToLower();

            if (string.IsNullOrEmpty(a))
            {
                if (!string.IsNullOrEmpty(b))
                {
                    return b.Length;
                }
                return 0;
            }

            if (string.IsNullOrEmpty(b))
            {
                if (!string.IsNullOrEmpty(a))
                {
                    return a.Length;
                }
                return 0;
            }

            Int32 cost;
            Int32[,] d = new int[a.Length + 1, b.Length + 1];
            Int32 min1;
            Int32 min2;
            Int32 min3;

            for (Int32 i = 0; i <= d.GetUpperBound(0); i += 1)
            {
                d[i, 0] = i;
            }

            for (Int32 i = 0; i <= d.GetUpperBound(1); i += 1)
            {
                d[0, i] = i;
            }

            for (Int32 i = 1; i <= d.GetUpperBound(0); i += 1)
            {
                for (Int32 j = 1; j <= d.GetUpperBound(1); j += 1)
                {
                    cost = Convert.ToInt32(!(a[i - 1] == b[j - 1]));

                    min1 = d[i - 1, j] + 1;
                    min2 = d[i, j - 1] + 1;
                    min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }

            return d[d.GetUpperBound(0), d.GetUpperBound(1)];

        }

        private static string getCode(string entity)
        {
            _metaphone.ComputeKeys(entity);
            return _metaphone.PrimaryKey;
        }
    }
}
