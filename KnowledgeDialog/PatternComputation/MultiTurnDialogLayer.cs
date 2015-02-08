using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    public class MultiTurnDialogLayer : ExplicitLayer
    {
        public readonly static string SentenceParent = "sentence";

        public readonly static string HasWordRelation = "has word";

        public readonly static string NextRelation = "next";

        /// <summary>
        /// Count of sentences within the dialog.
        /// </summary>
        private int _activeSentenceIndex;

        /// <summary>
        /// Index to next added word.
        /// </summary>
        private int _nextWordIndex;

        /// <summary>
        /// Context nodes of input that were used in current turn.
        /// </summary>
        private readonly HashSet<NodeReference> _currentInputContextNodes = new HashSet<NodeReference>();

        /// <summary>
        /// Context nodes of input that were used in current turn.
        /// </summary>
        public IEnumerable<NodeReference> CurrentInputContextNodes { get { return _currentInputContextNodes; } }

        internal void ActivateNewSentence()
        {
            var active = CreateReference(ComposedGraph.Active);
            var previousSentence = getSentenceNode(_activeSentenceIndex);

            if (_activeSentenceIndex > 0)
            {
                //remove old sentence index
                RemoveEdge(previousSentence, ComposedGraph.HasFlag, active);
            }
            ++_activeSentenceIndex;
            _nextWordIndex = 0;
            _currentInputContextNodes.Clear();

            //add new active sentence node
            var newSentence = getSentenceNode(_activeSentenceIndex);
            AddEdge(newSentence, ComposedGraph.HasFlag, active);
            AddEdge(previousSentence, NextRelation, newSentence);
            //AddEdge(newSentence, ComposedGraph.IsRelation, CreateReference(SentenceParent));
        }

        internal void AddSentenceWord(string word)
        {
            var previousWord = getWordPlaceholder(_activeSentenceIndex, _nextWordIndex - 1);
            var currentWord = getWordPlaceholder(_activeSentenceIndex, _nextWordIndex);
            var currentSentence = getSentenceNode(_activeSentenceIndex);

            //add word into graph
            var wordContextNode = CreateReference(word);
            _currentInputContextNodes.Add(wordContextNode);

            AddEdge(currentSentence, HasWordRelation, currentWord);
            AddEdge(currentWord, ComposedGraph.IsRelation, wordContextNode);
            if (_nextWordIndex > 0)
                //threre was previous word in sentence
                AddEdge(previousWord, NextRelation, currentWord);

            ++_nextWordIndex;
        }

        private NodeReference getSentenceNode(int index)
        {
            return CreateReference("s" + index);
        }

        private NodeReference getWordPlaceholder(int sentenceIndex, int wordIndex)
        {
            return CreateReference("s" + sentenceIndex + ".w" + wordIndex);
        }
    }
}
