using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class InterpretationsFactory
    {
        internal static readonly int MaxSearchWidth = 1000;

        internal static readonly int MaxConstraintLength = 4;

        internal static readonly int MaxTopicSelectorLength = 8;

        internal readonly NodeReference CorrectAnswerNode;

        internal readonly Interpretation ContractedInterpretation;


        private readonly ParsedUtterance _parsedQuestion;

        private readonly bool _isBasedOnContext;

        public InterpretationsFactory(Dialog.ParsedUtterance parsedQuestion, bool isBasedOnContext, NodeReference correctAnswerNode)
        {
            if (isBasedOnContext)
                throw new NotImplementedException();

            _parsedQuestion = parsedQuestion;
            _isBasedOnContext = isBasedOnContext;
            CorrectAnswerNode = correctAnswerNode;
            ContractedInterpretation = new Interpretation(new[] { new InsertPoolRule(correctAnswerNode) });

        }

        internal FeatureInstance GetSimpleFeatureInstance()
        {
            return SimpleFeatureGenerator.CreateSimpleFeatureInstance(_parsedQuestion);
        }

        internal TopicSelector GetTopicSelector(ComposedGraph graph)
        {
            return new TopicSelector(graph, CorrectAnswerNode);
        }

        internal ConstraintSelector GetConstraintSelector(ComposedGraph graph, IEnumerable<NodeReference> selectedNodes)
        {
            return new ConstraintSelector(graph, CorrectAnswerNode, selectedNodes);
        }
    }

    class TopicSelector
    {
        private readonly PathFactory _factory;

        private readonly List<PoolRuleBase> _rules = new List<PoolRuleBase>();

        private readonly HashSet<NodeReference> _selectedNodes = new HashSet<NodeReference>();

        internal int SelectedNodesCount { get { return _selectedNodes.Count; } }

        internal IEnumerable<NodeReference> SelectedNodes { get { return _selectedNodes; } }

        internal IEnumerable<PoolRuleBase> Rules { get { return _rules; } }

        internal NodeReference TargetNode { get { return _factory.StartingNode; } }

        internal TopicSelector(ComposedGraph graph, NodeReference targetNode)
        {
            _factory = new PathFactory(targetNode, graph, false, InterpretationsFactory.MaxSearchWidth, InterpretationsFactory.MaxTopicSelectorLength);
        }

        internal bool MoveNext()
        {
            _rules.Clear();
            _selectedNodes.Clear();


            var nextSegment = findNextSegment();
            if (nextSegment == null)
                //there are no more available segments.
                return false;

            _factory.Enqueue(nextSegment);

            _rules.AddRange(createRule(nextSegment));
            _selectedNodes.UnionWith(findSelectedNodes(nextSegment));


            ConsoleServices.Print("NEW TOPIC", Rules);
            ConsoleServices.PrintEmptyLine();

            return true;
        }

        private PathSegment findNextSegment()
        {
            while (_factory.HasNextPath)
            {
                var nextSegment = _factory.GetNextSegment();
                if (_factory.Graph.ContainsLoop(new[] { _factory.StartingNode },nextSegment.GetReversedEdges()))
                    //path contains loop
                    continue;

                return nextSegment;
            }
            return null;
        }

        /// <summary>
        /// Creates rules from given segment.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private IEnumerable<PoolRuleBase> createRule(PathSegment segment)
        {
            var insertedNode = segment.Node;

            yield return new InsertPoolRule(insertedNode);

            if (insertedNode != _factory.StartingNode)
                //we are not inserting the target node
                yield return new TransformPoolRule(segment);
        }

        private IEnumerable<NodeReference> findSelectedNodes(PathSegment segment)
        {
            if (segment.Node == TargetNode)
                //we are just inserting the correct node.
                return new[] { TargetNode };

            var edges = segment.GetInvertedEdges();
            var selectedNodes = _factory.Graph.GetForwardTargets(new[] { segment.Node }, edges);

            if (!selectedNodes.Contains(TargetNode))
                throw new NotSupportedException("Target node has to be present");

            return selectedNodes;
        }
    }

    class ConstraintSelector
    {
        private readonly PathFactory _factory;

        private readonly List<ConstraintPoolRule[]> _completeRules = new List<ConstraintPoolRule[]>();

        private readonly HashSet<NodeReference> _originalRemainingNodes;

        /// <summary>
        /// Sequence of rules which has been generated.
        /// </summary>
        private readonly List<ConstraintPoolRule[]> _incompleteRuleSequence = new List<ConstraintPoolRule[]>();

        /// <summary>
        /// Remaining nodes corresponding to incomplete rule sequence
        /// </summary>
        private readonly List<HashSet<NodeReference>> _remainingNodes = new List<HashSet<NodeReference>>();

        /// <summary>
        /// Last index which was combined with the <see cref="_combinationRule"/>.
        /// </summary>
        private int _combinationIndex = 0;

        /// <summary>
        /// Rule which will be combined with the sequence.
        /// </summary>
        private ConstraintPoolRule[] _combinationRule = null;

        /// <summary>
        /// Determine whether first rule is going to be generated.
        /// </summary>
        private bool _isFirst = true;

        private HashSet<NodeReference> _combinationNodes = null;

        internal bool IsEnd { get; private set; }

        internal ComposedGraph Graph { get { return _factory.Graph; } }

        internal IEnumerable<ConstraintPoolRule> Rules { get { return _completeRules.Count == 0 ? new ConstraintPoolRule[0] : _completeRules[_completeRules.Count - 1]; } }

        internal ConstraintSelector(ComposedGraph graph, NodeReference targetNode, IEnumerable<NodeReference> selectedNodes)
        {
            _factory = new PathFactory(targetNode, graph, false, InterpretationsFactory.MaxSearchWidth, InterpretationsFactory.MaxConstraintLength);

            if (!selectedNodes.Contains(targetNode))
                throw new NotSupportedException("Cannot constraint nodes without target");

            _originalRemainingNodes = new HashSet<NodeReference>(selectedNodes.Except(new[] { targetNode }));
        }

        internal bool MoveNext()
        {
            while (!IsEnd)
            {
                if (tryFindNextConstraint())
                    return true;
            }

            return false;
        }

        #region Constraint discovering

        private bool tryFindNextConstraint()
        {
            var wasFirst = _isFirst;
            _isFirst = false;
            if (_originalRemainingNodes.Count == 0 || !_factory.HasNextPath)
            {
                //there is no need for other than empty rule
                if (!wasFirst)
                    IsEnd = true;

                return wasFirst;
            }

            var lastCompleteRuleCount = _completeRules.Count;
            if (_combinationIndex < 1)
            {
                //create new rule from next constraint path
                var pathSegment = _factory.GetNextSegment();
                if (pathSegment == null)
                    return false;

                var constraint = createConstraint(pathSegment);
                var newRule = new ConstraintPoolRule[] { constraint };

                var remainingNodes = findRemainingNodes(constraint);
                var isTrivialRule = remainingNodes.Count == _originalRemainingNodes.Count;
                if (isTrivialRule || Graph.ContainsLoop(new[] { _factory.StartingNode }, pathSegment.GetReversedEdges()))
                    //we don't need any trivial rule
                    return false;

                _factory.Enqueue(pathSegment);
                addRule(newRule, remainingNodes);

                //set combination for next rules
                _combinationIndex = _incompleteRuleSequence.Count - 1;
                _combinationRule = newRule;
                _combinationNodes = remainingNodes;
            }
            else
            {
                --_combinationIndex;
                //combine with previous rule
                var ruleToCombine = _incompleteRuleSequence[_combinationIndex];
                var nodesToCombine = _remainingNodes[_combinationIndex];

                var combinedRule = combineRules(_combinationRule, ruleToCombine);
                var combinedNodes = combineNodes(_combinationNodes, nodesToCombine);

                var isTrivialExtension = combinedNodes.Count == _combinationNodes.Count || combinedNodes.Count == nodesToCombine.Count;
                if (!isTrivialExtension)
                    addRule(combinedRule, combinedNodes);
            }

            //detect whether new complete rule has been found
            return _completeRules.Count > lastCompleteRuleCount;
        }

        private ConstraintPoolRule[] combineRules(ConstraintPoolRule[] combinationRule, ConstraintPoolRule[] ruleToCombine)
        {
            var result = new ConstraintPoolRule[combinationRule.Length + ruleToCombine.Length];

            for (var i = 0; i < combinationRule.Length; ++i)
                result[i] = combinationRule[i];

            for (var i = 0; i < ruleToCombine.Length; ++i)
                result[i + combinationRule.Length] = ruleToCombine[i];

            return result;
        }

        private HashSet<NodeReference> combineNodes(HashSet<NodeReference> combinationNodes, HashSet<NodeReference> nodesToCombine)
        {
            var smallerSet = combinationNodes.Count < nodesToCombine.Count ? combinationNodes : nodesToCombine;
            var largerSet = smallerSet == combinationNodes ? nodesToCombine : combinationNodes;

            var copy = new HashSet<NodeReference>(smallerSet);
            copy.IntersectWith(largerSet);

            return copy;
        }

        private HashSet<NodeReference> findRemainingNodes(ConstraintPoolRule rule)
        {
            var pool = new ContextPool(Graph);
            //TODO optimize filling the pool
            pool.Insert(_originalRemainingNodes.ToArray());
            rule.Execute(pool);

            //take only those remaining nodes which have been selected
            var remainingNodes = new HashSet<NodeReference>(pool.ActiveNodes);
            return remainingNodes;
        }
        #endregion

        #region Private utilities

        private void addRule(ConstraintPoolRule[] rules, HashSet<NodeReference> remainingNodes)
        {
            var isCompleteRule = remainingNodes.Count == 0;
            if (isCompleteRule)
            {
                //complete rules don't need more extending
                _completeRules.Add(rules);
            }
            else
            {
                //rule is incomplete
                _incompleteRuleSequence.Add(rules);
                _remainingNodes.Add(remainingNodes);
            }

            ConsoleServices.Print("RULES", rules, remainingNodes);
            ConsoleServices.PrintEmptyLine();
        }

        private ConstraintPoolRule createConstraint(PathSegment constraintPath)
        {
            return new ConstraintPoolRule(constraintPath);
        }

        #endregion
    }
}
