﻿using System;
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

        private static readonly Regex _inputSanitizer = new Regex("[,.?<>;\"-]", RegexOptions.Compiled);

        private static readonly Regex _spaceSanitizer = new Regex(@"[ ]{2,}", RegexOptions.Compiled);

        private static readonly Dictionary<string, List<string>> _indexedEntities = new Dictionary<string, List<string>>();

        private static readonly DoubleMetaphone _metaphone = new DoubleMetaphone();

        public static IEnumerable<string> NonInformativeWords { get { return _nonInformativeWords; } }

        /// <summary>
        /// Words that does not give any explanatory information.
        /// </summary>
        static readonly HashSet<string> _nonInformativeWords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "of","for","a","to", "by","about","the","whether","what","how","who","which","when","why", "with", "without", "whatever", "whoever", "whenever", "however", "then", "than", "please", "correct", "whats", "what's", "s",

            "and", "or",

            "answer", "question", "questioner" ,"ask" ,"say", "saying" ,"asking",

            "wonder", "wondering", "know", "knowing", "want",

            "curios", "interested", "interesting", "interest", "answer", "answering", "look", "looking", "try", "trying", "find", "finding",

            "think", "thinks", "was", "were",

            "you", "it", "me", "i", "m", "we",

            "can", "could", "will", "would", "tell", "give", "name", "think", "thing",

            "up", "down",

            "dont", "idk", "hi", "hey", "hei", "on","any","this","hate",
            "shit","stupid","idiot", "is","in", "im", "is", "that","no","yes", "yeah","are","my","your","his","her","him", "their","they","she","he","it","there","that","this","here","like","so","never","ever","always","any","some","anytime","sometime","from",
            "has","have","had","will", "u", "not","lol","oh", "asl",
            "well","bad","good","great", "same","different","another", "just","ha","haha","even","odd", "other", "another","whose"
        };


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

        public static bool IsInformativeWord(string word)
        {
            word = word.ToLowerInvariant().Replace("'", "");
            if (word.Length < 3)
                return false;

            return !_nonInformativeWords.Contains(word) && !word.Contains("fuck");
        }

        public static ParsedUtterance Parse(string originalUtterance)
        {
            var sanitizedUtterance = _inputSanitizer.Replace(originalUtterance, " ");
            sanitizedUtterance = _spaceSanitizer.Replace(sanitizedUtterance, " ").Trim();
            return parseExpression(sanitizedUtterance, originalUtterance);
        }

        private static ParsedUtterance parseExpression(string sanitizedSentence, string originalUtterance)
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

            return new ParsedUtterance(originalUtterance, parsedWords);
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
                var distance = Levenshtein(ngram, entity);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    candidate = entity;
                }
            }

            return Tuple.Create(candidate, minDistance);
        }

        public static int Levenshtein(string a, string b)
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
