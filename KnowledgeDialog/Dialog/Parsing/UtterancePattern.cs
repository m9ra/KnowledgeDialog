using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Parsing
{

    class UtterancePattern
    {
        private readonly HashSet<string>[] _wordSets;

        private readonly string[] _sentenceKeys;

        private readonly string[] _patternWords;

        public readonly static string GroupPrefix = "$";

        public readonly static string SentencePrefix = "#";

        internal int Length { get { return _wordSets.Length; } }

        internal UtterancePattern(string patternDefinition, PatternConfiguration configuration)
        {
            _patternWords = patternDefinition.Split(' ').ToArray();
            _sentenceKeys = new string[_patternWords.Length];
            _wordSets = new HashSet<string>[_patternWords.Length];

            for (var wordIndex = 0; wordIndex < _patternWords.Length; ++wordIndex)
            {
                var word = _patternWords[wordIndex];
                var targetSet = generateTargetSet(word, configuration);
                if (targetSet != null)
                {
                    _wordSets[wordIndex] = targetSet;
                    continue;
                }

                if (word.StartsWith(SentencePrefix))
                {
                    _sentenceKeys[wordIndex] = word.Substring(SentencePrefix.Length);
                    continue;
                }

                throw new NotSupportedException("Given construction '" + word + "' is not supported yet");
            }
        }

        internal PatternState InitialState()
        {
            return new PatternState(this);
        }

        private HashSet<string> generateTargetSet(string word, PatternConfiguration configuration)
        {
            if (word.StartsWith(GroupPrefix))
            {
                var groupSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                var groupKey = word.Substring(GroupPrefix.Length);
                groupSet.UnionWith(configuration.GetGroup(groupKey));
                return groupSet;
            }

            if (word.StartsWith(SentencePrefix))
                return null;

            var singleWordSet = new HashSet<string>();
            singleWordSet.Add(word);

            return singleWordSet;
        }

        internal bool IsAllowed(int stateIndex, string inputWord)
        {
            if (_wordSets[stateIndex] == null)
                //there is no explicit set of words but it is possible to attach them into sentence
                return GetSentenceKey(stateIndex) != null;

            return _wordSets[stateIndex].Contains(inputWord);
        }

        internal string GetSentenceKey(int stateIndex)
        {
            return _sentenceKeys[stateIndex];
        }
    }
}
