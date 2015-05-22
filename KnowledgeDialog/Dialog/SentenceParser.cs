using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace KnowledgeDialog.Dialog
{
    public static class SentenceParser
    {
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

        internal static ParsedSentence Parse(string sentence)
        {
            sentence = _inputSanitizer.Replace(sentence, " ");
            sentence = _spaceSanitizer.Replace(sentence, " ").Trim();
            var validEntities = findEntities(sentence);
            return parseSentence(sentence, validEntities);
        }

        private static List<StringSearchResult> findEntities(string sentence)
        {
            var foundEntities = getRawEntities(sentence);


            //try to find longest entities as posible
            var validEntities = new List<StringSearchResult>();
            foreach (var foundEntity in foundEntities.OrderByDescending(e => e.Keyword.Length))
            {
                //optimistic claim
                var isValid = true;
                foreach (var validEntity in validEntities)
                {
                    //try to proove invalidity
                    var startIndex = foundEntity.Index;
                    var endIndex = startIndex + foundEntity.Keyword.Length;

                    var validStartIndex = validEntity.Index;
                    var validEndIndex = validStartIndex + validEntity.Keyword.Length;

                    var startBefore = startIndex < validStartIndex;
                    var endBefore = endIndex < validStartIndex;

                    var startAfter = startIndex > validEndIndex;
                    var endAfter = endIndex > validEndIndex;

                    isValid = isValid && ((startBefore == endBefore) && (startAfter == endAfter) && (startBefore || startAfter));
                }

                if (isValid)
                    validEntities.Add(foundEntity);
            }
            return validEntities;
        }

        private static IEnumerable<StringSearchResult> getRawEntities(string sentence)
        {
            var unigrams = sentence.Split(' ');
            var maxNGram = unigrams.Length;

            var result = new List<StringSearchResult>();
            for (var n = maxNGram; n > 0; --n)
            {
                //each ngram that is longer than 1 may be named entity
                for (var ngramOffset = 0; ngramOffset < unigrams.Length - n + 1; ++ngramOffset)
                {
                    var ngram = new StringBuilder();
                    for (var i = 0; i < n; ++i)
                    {
                        if (i > 0)
                            ngram.Append(' ');

                        var word = unigrams[ngramOffset + i];
                        ngram.Append(word);
                    }

                    var ngramString = ngram.ToString();
                    var match = getBestMatch(ngramString);

                    if (match.Item2 <= 2.0)
                    {
                        //TODO this supposes that word has appeared only once
                        var startIndex = sentence.IndexOf(unigrams[ngramOffset]);

                        var endWord = unigrams[ngramOffset + n - 1];
                        var endIndex = sentence.IndexOf(endWord) + endWord.Length;

                        var original = sentence.Substring(startIndex, endIndex - startIndex);
                        var searchResult = new StringSearchResult(startIndex, match.Item1, original);
                        result.Add(searchResult);
                    }
                }
            }

            return result;
        }

        private static Tuple<string, double> getBestMatch(string ngram)
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

        private static ParsedSentence parseSentence(string sentence, List<StringSearchResult> validEntities)
        {
            var parsedWords = new List<string>();

            var currentEntityIndex = 0;
            var currentIndex = 0;
            while (currentIndex < sentence.Length)
            {
                var nextEntityStart = sentence.Length;
                var hasEntity = currentEntityIndex < validEntities.Count;
                if (hasEntity)
                {
                    nextEntityStart = validEntities[currentEntityIndex].Index;
                }

                var currentSentencePart = sentence.Substring(currentIndex, nextEntityStart - currentIndex);
                if (currentSentencePart != "")
                    parsedWords.AddRange(currentSentencePart.Trim().Split(' '));

                currentIndex += currentSentencePart.Length;
                if (hasEntity)
                {
                    var entity = validEntities[currentEntityIndex];
                    parsedWords.Add(entity.Keyword);
                    currentIndex = nextEntityStart + entity.OriginalText.Length;
                    ++currentEntityIndex;
                }
            }

            return new ParsedSentence(sentence, parsedWords);
        }

        private static string getCode(string entity)
        {
            _metaphone.ComputeKeys(entity);
            return _metaphone.PrimaryKey;
        }
    }
}
