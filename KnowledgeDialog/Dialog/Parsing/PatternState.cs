using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Parsing
{
    class PatternState
    {
        internal readonly UtterancePattern OriginalPattern;

        internal readonly PatternState Parent;

        internal readonly int StateIndex;

        internal readonly int CoveredOffset;

        internal bool IsFinished { get { return StateIndex >= OriginalPattern.Length; } }

        internal readonly IEnumerable<IEnumerable<string>> Substitutions;

        internal IEnumerable<int> CoveredIndexes
        {
            get
            {
                var alreadyCoveredOffsets = new HashSet<int>();
                var currentState = this;
                while (currentState != null)
                {
                    var offset = currentState.CoveredOffset;
                    if (alreadyCoveredOffsets.Add(offset))
                        yield return currentState.CoveredOffset;

                    currentState = currentState.Parent;
                }
            }
        }

        public PatternState(UtterancePattern originalPattern, int coveredOffset)
        {
            Parent = null;
            CoveredOffset = coveredOffset;
            OriginalPattern = originalPattern;
            Substitutions = new IEnumerable<string>[] { };
        }

        private PatternState(PatternState parent, int index, int coveredOffset, IEnumerable<IEnumerable<string>> substitutions)
        {
            Parent = parent;
            CoveredOffset = coveredOffset;
            OriginalPattern = parent.OriginalPattern;
            StateIndex = index;
            Substitutions = substitutions;
        }

        internal IEnumerable<PatternState> GetNextStates(string inputWord, int wordOffset)
        {
            var result = new List<PatternState>();
            if (StateIndex >= OriginalPattern.Length)
                //pattern cannot accept other words
                return result;

            var isInGroup = OriginalPattern.IsAllowed(StateIndex, inputWord);
            var sentenceKey = OriginalPattern.GetSentenceKey(StateIndex);

            if (!isInGroup)
                //this pattern path doesn't match
                return result;

            //we may want to add word into sentence
            var sentenceSubstitutions = new List<IEnumerable<string>>(Substitutions);
            if (sentenceSubstitutions.Count >= StateIndex)
                //new entry
                sentenceSubstitutions.Add(new string[0]);

            sentenceSubstitutions[StateIndex] = sentenceSubstitutions[StateIndex].Concat(new[] { inputWord });

            if (sentenceKey != null)
            {
                //keep at same index
                var sentenceState = new PatternState(this, StateIndex, wordOffset, sentenceSubstitutions);
                result.Add(sentenceState);
            }

            var nextWordState = new PatternState(this, StateIndex + 1, wordOffset, sentenceSubstitutions);
            result.Add(nextWordState);
            return result;
        }
    }
}
