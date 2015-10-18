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
        internal static readonly int MaxSearchWidth = 100;

        internal readonly NodeReference CorrectAnswerNode;

        internal readonly Interpretation ContractedInterpretation;


        private readonly ParsedUtterance _parsedQuestion;
        private readonly bool _isBasedOnContext;
        private readonly IEnumerable<NodeReference> _context;

        public InterpretationsFactory(Dialog.ParsedUtterance parsedQuestion, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            if (isBasedOnContext)
                throw new NotImplementedException();

            _parsedQuestion = parsedQuestion;
            _isBasedOnContext = isBasedOnContext;
            CorrectAnswerNode = correctAnswerNode;            
            ContractedInterpretation = new Interpretation(new[] { new InsertPoolRule(correctAnswerNode) });

            _context = context;
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
            _factory = new PathFactory(targetNode, graph, InterpretationsFactory.MaxSearchWidth);
        }

        internal bool MoveNext()
        {
            _rules.Clear();
            _selectedNodes.Clear();

            if (!_factory.HasNextPath)
                //there are no more topic rules available
                return false;



            var nextSegment = _factory.GetNextSegment();

            _rules.AddRange(createRules(nextSegment));
            _selectedNodes.UnionWith(findSelectedNodes(nextSegment));

            return true;
        }

        /// <summary>
        /// Creates rules from given segment.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private IEnumerable<PoolRuleBase> createRules(PathSegment segment)
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

        /// <summary>
        /// Sequence of rules which has been generated.
        /// </summary>
        private readonly List<PoolRuleBase[]> _ruleSequence = new List<PoolRuleBase[]>();

        private readonly HashSet<NodeReference> _originalRemainingNodes;

        private readonly List<HashSet<NodeReference>> _remainingNodes = new List<HashSet<NodeReference>>();

        /// <summary>
        /// Last index which was combined with the <see cref="_combinationRule"/>.
        /// </summary>
        private int _combinationIndex = 0;

        /// <summary>
        /// Rule which will be combined with the sequence.
        /// </summary>
        private PoolRuleBase[] _combinationRule = null;

        private HashSet<NodeReference> _combinationNodes = null;

        internal bool IsEnd { get { return _combinationIndex <= 0 && !_factory.HasNextPath; } }

        internal ComposedGraph Graph { get { return _factory.Graph; } }

        internal IEnumerable<PoolRuleBase> Rules { get { return _ruleSequence[_ruleSequence.Count - 1]; } }

        internal ConstraintSelector(ComposedGraph graph, NodeReference targetNode, IEnumerable<NodeReference> selectedNodes)
        {
            _factory = new PathFactory(targetNode, graph, InterpretationsFactory.MaxSearchWidth);

            if (!selectedNodes.Contains(targetNode))
                throw new NotSupportedException("Cannot constraint nodes without target");

            _originalRemainingNodes = new HashSet<NodeReference>(selectedNodes.Except(new[] { targetNode }));

            MoveNext();
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
            --_combinationIndex;
            if (_combinationIndex < 0)
            {
                //create new rule from next constraint path
                var pathSegment = _factory.GetNextSegment();
                var newRule = createRule(pathSegment);
                var remainingNodes = findRemainingNodes(pathSegment);
                addRule(newRule, remainingNodes);

                //set combination for next rules
                _combinationIndex = _ruleSequence.Count - 1;
                _combinationRule = newRule;
                _combinationNodes = remainingNodes;
            }
            else
            {
                //combine with previous rule
                var ruleToCombine = _ruleSequence[_combinationIndex];
                var nodesToCombine = _remainingNodes[_combinationIndex];

                var combinedRule = combineRules(_combinationRule, ruleToCombine);
                var combinedNodes = combineNodes(_combinationNodes, nodesToCombine);

                addRule(combinedRule, combinedNodes);
            }

            //if there is no node remaining - we have found the constraint
            return _remainingNodes[_remainingNodes.Count - 1].Count == 0;
        }

        private PoolRuleBase[] combineRules(PoolRuleBase[] combinationRule, PoolRuleBase[] ruleToCombine)
        {
            var result = new PoolRuleBase[combinationRule.Length + ruleToCombine.Length];

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

        private HashSet<NodeReference> findRemainingNodes(PathSegment constraintPath)
        {
            var edges = constraintPath.GetInvertedEdges();
            var selectedNodes = _factory.Graph.GetForwardTargets(new[] { constraintPath.Node }, edges);

            //take only those remaining nodes which have been selected
            var remainingNodes = new HashSet<NodeReference>(_originalRemainingNodes);
            remainingNodes.IntersectWith(selectedNodes);
            return remainingNodes;
        }
        #endregion

        #region Private utilities

        private void addRule(PoolRuleBase[] rules, HashSet<NodeReference> remainingNodes)
        {
            _ruleSequence.Add(rules);
            _remainingNodes.Add(remainingNodes);
        }

        private PoolRuleBase[] createRule(PathSegment constraintPath)
        {
            return new PoolRuleBase[] { new ConstraintPoolRule(constraintPath) };
        }

        #endregion
    }
}
