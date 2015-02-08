using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation.PartialMatching
{
    class PartialEvaluation : IEvaluation
    {
        private readonly KnowledgeGroup _group;

        private readonly SubgraphSnapshot _snapshot;

        private readonly ComposedGraph _context;

        private readonly Dictionary<Tuple<NodeRestriction, NodeReference>, double> _nodesScore = new Dictionary<Tuple<NodeRestriction, NodeReference>, double>();

        private readonly Dictionary<NodeRestriction, NodeReference> _substitutions = new Dictionary<NodeRestriction, NodeReference>();

        private readonly Dictionary<NodeReference, IEnumerable<KeyValuePair<string, bool>>> _patternEdges = new Dictionary<NodeReference, IEnumerable<KeyValuePair<string, bool>>>();

        private readonly Dictionary<NodeReference, NodeRestriction> _restrictions = new Dictionary<NodeReference, NodeRestriction>();

        private readonly Dictionary<NodeReference, NodeReference> _nodeSubstitutions = new Dictionary<NodeReference, NodeReference>();


        public PartialEvaluation(KnowledgeGroup group, ComposedGraph context)
        {
            var snapshot = SubgraphSnapshot.InduceFromGroup(group, context);
            _group = group;
            _context = context;
            _snapshot = snapshot;
            _restrictions = GroupEvaluation.CreateRestrictions(group);

            initializeScore(snapshot);
            initializeSubstitutions();
            buildSubstitutionIndex();
        }

        public NodeReference GetSubstitution(NodeReference node)
        {
            NodeReference substitutedNode;
            _nodeSubstitutions.TryGetValue(node, out substitutedNode);

            return substitutedNode;
        }

        private void buildSubstitutionIndex()
        {
            foreach (var pair in _substitutions)
            {
                _nodeSubstitutions[pair.Key.BaseNode] = pair.Value;
            }
        }

        private void initializeSubstitutions()
        {
            var firstNode = _snapshot.EntryNode;
            var firstRestriction = _restrictions[firstNode];
            _substitutions[firstRestriction] = firstNode;

            var processedRestrictions = new Queue<NodeRestriction>();
            processedRestrictions.Enqueue(firstRestriction);

            while (processedRestrictions.Count > 0)
            {
                var currentRestriction = processedRestrictions.Dequeue();

                for (int i = 0; i < currentRestriction.TargetsCount; ++i)
                {
                    var target = currentRestriction.GetTarget(i);

                    if (_substitutions.ContainsKey(target))
                        //this restriction is already substituted
                        continue;

                    var edge = currentRestriction.GetEdge(i);
                    var isOutcomming = currentRestriction.IsOutDirection(i);
                    var substitutedNode = _substitutions[currentRestriction];

                    var bestScore = Double.NegativeInfinity;
                    NodeReference bestNode = null;
                    foreach (var node in _snapshot.GetNodes(substitutedNode, edge, isOutcomming))
                    {
                        var key = Tuple.Create(target, node);
                        var score = _nodesScore[key];
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestNode = node;
                        }
                    }

                    _substitutions.Add(target, bestNode);
                    if (bestNode != null)
                    {
                        processedRestrictions.Enqueue(target);
                    }
                }
            }
        }

        private void initializeScore(SubgraphSnapshot snapshot)
        {
            var restrictions = preorderedRestrictions(snapshot).Reverse().ToArray();
            foreach (var restriction in restrictions)
            {
                foreach (var alternative in snapshot.GetAssociatedNodes(restriction.BaseNode))
                {
                    var key = Tuple.Create(restriction, alternative);
                    if (_nodesScore.ContainsKey(key))
                        continue;

                    var score = getNeighboursScore(alternative, restriction);
                    _nodesScore.Add(key, score);
                }
            }
        }

        private IEnumerable<NodeRestriction> preorderedRestrictions(SubgraphSnapshot snapshot)
        {
            var entryRestriction = _restrictions[snapshot.EntryNode];
            var restrictionsQueue = new Queue<NodeRestriction>();
            var enqueuedRestrictions = new HashSet<NodeRestriction>();
            enqueuedRestrictions.Add(entryRestriction);
            restrictionsQueue.Enqueue(entryRestriction);

            yield return entryRestriction;

            while (restrictionsQueue.Count > 0)
            {
                var currentRestriction = restrictionsQueue.Dequeue();
                yield return currentRestriction;

                //expand to all neighbouring  restrictions
                for (var i = 0; i < currentRestriction.TargetsCount; ++i)
                {
                    var target = currentRestriction.GetTarget(i);
                    if (enqueuedRestrictions.Add(target))
                        restrictionsQueue.Enqueue(target);
                }
            }
        }

        private double getNeighboursScore(NodeReference node, NodeRestriction restriction)
        {
            var key = Tuple.Create(restriction, node);
            double score;
            if (_nodesScore.TryGetValue(key, out score))
                return score;


            var targets = _snapshot.GetTargets(node);

            score = getNodeScore(node, restriction.BaseNode);
            for (var i = 0; i < restriction.TargetsCount; ++i)
            {
                var target = restriction.GetTarget(i);
                var edge = restriction.GetEdge(i);
                var isOut = restriction.IsOutDirection(i);
                var candidates = _snapshot.GetNodes(node, edge, isOut);

                var bestScore=0.0;                
                foreach (var candidate in candidates)
                {
                    var targetKey = Tuple.Create(target, candidate);
                    double neighbourScore;
                    _nodesScore.TryGetValue(targetKey, out neighbourScore);
                    if (neighbourScore > bestScore)
                        bestScore = neighbourScore;
                }

                score += bestScore;
            }
            return score;
        }

        private double getNodeScore(NodeReference node1, NodeReference node2)
        {
            var shortestPath = _context.GetPaths(node1, node2, 1000, 1000).FirstOrDefault();
            if (shortestPath == null)
                return 0;

            return 1.0 / (1 + shortestPath.Length);
        }
    }
}
