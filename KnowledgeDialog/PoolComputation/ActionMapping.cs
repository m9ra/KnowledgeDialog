using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class ActionMapping
    {
        List<IPoolAction> _actions = new List<IPoolAction>();

        List<ActionGroup> _groups = new List<ActionGroup>();

        Dictionary<string, IEnumerable<IPoolAction>> _wordIndex = new Dictionary<string, IEnumerable<IPoolAction>>();

        Dictionary<string, HashSet<GroupContext>> _relativeGroupIndex = new Dictionary<string, HashSet<GroupContext>>();

        public IEnumerable<PoolHypothesis> GetActions(string utterance, ContextPool pool)
        {
            var words = utterance.Split(' ');

            var substitutions = new Dictionary<NodeReference, NodeReference>();
            var actions = new List<IPoolAction>();
            var skipWords = new HashSet<string>();
            while (true)
            {
                var scoreTable = getMappingScore(words, skipWords);
                if (scoreTable.Count == 0)
                    //we cannot map other information from the input
                    break;

                var sortedTable = scoreTable.OrderByDescending(p => p.Value);
                foreach (var scoredGroup in sortedTable)
                {
                    var absoluteContextGroup = scoredGroup.Key;
                    var targetWord = words[absoluteContextGroup.Offset];
                    if (pool.Graph.HasEvidence(targetWord))
                    {
                        var representingAction = absoluteContextGroup.Group.Actions.First();
                        var substitutedNode = pool.Graph.GetNode(targetWord);
                        substitutions.Add(representingAction.SemanticOrigin.StartNode, substitutedNode);

                        actions.Add(representingAction);
                        skipWords.UnionWith(absoluteContextGroup.Group.RegisteredWords);

                        break;
                    }
                }
            }

            if (actions.Count == 0)
                return Enumerable.Empty<PoolHypothesis>();

            actions.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            return new[]{
                new PoolHypothesis(substitutions,actions)
            };
        }

        private Dictionary<GroupContext, double> getMappingScore(string[] words, HashSet<string> skipWords)
        {
            var scoreTable = new Dictionary<GroupContext, double>();
            for (var i = 0; i < words.Length; ++i)
            {
                var word = words[i];
                if (skipWords.Contains(word))
                    //skip the word
                    continue;

                HashSet<GroupContext> relativeGroupContexts;
                if (!_relativeGroupIndex.TryGetValue(word, out relativeGroupContexts))
                    continue;

                var gain = 1.0 / relativeGroupContexts.Count;
                foreach (var relativeGroupContext in relativeGroupContexts)
                {
                    var absoluteGroupContext = new GroupContext(relativeGroupContext.Group, relativeGroupContext.Offset + i);

                    double currentScore;
                    scoreTable.TryGetValue(absoluteGroupContext, out currentScore);
                    currentScore += gain;
                    scoreTable[absoluteGroupContext] = currentScore;
                }
            }
            return scoreTable;
        }

        private double getScore(string utterance, string pattern)
        {
            var words = utterance.Split(' ');
            var patternWords = utterance.Split(' ');

            var matchCount = 0;
            foreach (var word in words)
            {
                if (patternWords.Contains(word))
                    ++matchCount;
            }

            return 2.0 * matchCount / (words.Length + patternWords.Length);
        }

        internal void SetMapping(IEnumerable<IEnumerable<IPoolAction>> actionHypotheses)
        {
            if (actionHypotheses.Count() > 1)
                throw new NotSupportedException("Cannot accept multiple hypothesis");

            var clusters = new Dictionary<string, List<IPoolAction>>();
            foreach (var hypothesis in actionHypotheses)
            {
                var nodeWords = (from action in hypothesis select action.SemanticOrigin.StartNode.Data.ToString()).ToArray();

                foreach (var action in hypothesis)
                {
                    var words = getDeterminingWords(action, nodeWords).ToArray();
                    var group = getGroup(action);

                    for (var i = 0; i < words.Length; ++i)
                    {
                        var word = words[i];
                        group.Add(word, action);

                        registerGroup(word, words.Length - i - 1, group);
                    }
                }
            }

            foreach (var cluster in clusters)
            {
                _wordIndex.Add(cluster.Key, cluster.Value);
            }
        }

        private ActionGroup getGroup(IPoolAction action)
        {
            foreach (var group in _groups)
            {
                if (group.CanInclude(action))
                    return group;
            }

            var result = new ActionGroup();
            _groups.Add(result);

            return result;
        }

        private void registerGroup(string word, int relativeOffset, ActionGroup group)
        {
            HashSet<GroupContext> contexts;
            if (!_relativeGroupIndex.TryGetValue(word, out contexts))
                _relativeGroupIndex[word] = contexts = new HashSet<GroupContext>();

            var context = new GroupContext(group, relativeOffset);
            contexts.Add(context);
        }

        private IEnumerable<string> getDeterminingWords(IPoolAction action, IEnumerable<string> nodeWords)
        {
            //TODO this is not precise because of multiple same words, parsing,...
            var semantic = action.SemanticOrigin;
            var nodeWord = semantic.StartNode.Data.ToString();

            var prefixIndex = semantic.Utterance.IndexOf(nodeWord);
            var prefix = semantic.Utterance.Substring(0, prefixIndex);

            var previousNodeIndexMax = 0;
            var previousNodeWordMax = "";
            foreach (var previousNodeWord in nodeWords)
            {
                var index = prefix.IndexOf(previousNodeWord);
                if (index >= previousNodeIndexMax)
                {
                    previousNodeIndexMax = index;
                    previousNodeWordMax = previousNodeWord;
                }
            }

            var determiningPart = prefix.Substring(previousNodeIndexMax + previousNodeWordMax.Length, prefix.Length - previousNodeIndexMax - previousNodeWordMax.Length).Trim();

            var result = new List<string>();
            result.AddRange(determiningPart.Split(' '));
            result.Add(nodeWord);

            return result.Where(p => p.Length > 0);
        }
    }
}
