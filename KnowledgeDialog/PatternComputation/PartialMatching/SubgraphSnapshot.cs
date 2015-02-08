using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation.PartialMatching
{
    class SubgraphSnapshot
    {
        public readonly NodeReference EntryNode;

        private readonly Dictionary<NodeReference, List<NodeReference>> _associations = new Dictionary<NodeReference, List<NodeReference>>();

        private readonly Dictionary<Tuple<NodeReference, string, bool>, List<NodeReference>> _nodes = new Dictionary<Tuple<NodeReference, string, bool>, List<NodeReference>>();

        private readonly Dictionary<NodeReference, List<Tuple<NodeReference, string, bool>>> _targets = new Dictionary<NodeReference, List<Tuple<NodeReference, string, bool>>>();

        internal SubgraphSnapshot(NodeReference startNode)
        {
            EntryNode = startNode;
        }

        internal static SubgraphSnapshot InduceFromGroup(KnowledgeGroup group, ComposedGraph context)
        {
            //start from an active node
            var activeNode = context.GetNode(ComposedGraph.Active);
            var snapshot = new SubgraphSnapshot(activeNode);
            var restrictions = GroupEvaluation.CreateRestrictions(group);

            var activeRestriction = restrictions[activeNode];

            var restrictionsQueue = new Queue<NodeRestriction>();
            var enqueuedRestrictions = new HashSet<NodeRestriction>();
            enqueuedRestrictions.Add(activeRestriction);
            restrictionsQueue.Enqueue(activeRestriction);
            snapshot.Associate(activeNode, activeNode);

            while (restrictionsQueue.Count > 0)
            {
                var currentRestriction = restrictionsQueue.Dequeue();
                var currentAlternatives = snapshot.GetAssociatedNodes(currentRestriction.BaseNode);

                //extend every edge from restriction
                for (var i = 0; i < currentRestriction.TargetsCount; ++i)
                {
                    var edge = currentRestriction.GetEdge(i);
                    var isOut = currentRestriction.IsOutDirection(i);
                    var target = currentRestriction.GetTarget(i);

                    //shift every alternative by restriction edge
                    foreach (var alternative in currentAlternatives)
                    {
                        var nextAlternatives = context.Targets(alternative, edge, isOut);
                        foreach (var nextAlternative in nextAlternatives)
                        {
                            snapshot.Associate(nextAlternative, target.BaseNode);
                            snapshot.AddEdge(alternative, edge, isOut, nextAlternative);
                        }
                    }

                    //avoid multiple enqueueing
                    if (enqueuedRestrictions.Add(target))
                        restrictionsQueue.Enqueue(target);
                }
            }

            return snapshot;
        }

        /// <summary>
        /// Associate given node with given association.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="association"></param>
        private void Associate(NodeReference node, NodeReference association)
        {
            List<NodeReference> associatedNodes;
            if (!_associations.TryGetValue(association, out associatedNodes))
                _associations[association] = associatedNodes = new List<NodeReference>();

            associatedNodes.Add(node);
        }

        internal IEnumerable<NodeReference> GetAssociatedNodes(NodeReference association)
        {
            List<NodeReference> associatedNodes;
            if (!_associations.TryGetValue(association, out associatedNodes))
                return new NodeReference[0];

            return associatedNodes;
        }

        internal IEnumerable<NodeReference> GetNodes(NodeReference currentNode, string edge, bool isOutDirection)
        {
            List<NodeReference> nodes;
            if (!_nodes.TryGetValue(Tuple.Create(currentNode, edge, isOutDirection), out nodes))
                return new NodeReference[0];

            return nodes;
        }

        private void AddEdge(NodeReference node1, string edge, bool isOut, NodeReference node2)
        {
            addEdgeRaw(node1, edge, isOut, node2);
            addEdgeRaw(node2, edge, !isOut, node1);
        }

        private void addEdgeRaw(NodeReference node1, string edge, bool isOut, NodeReference node2)
        {
            var key = Tuple.Create(node1, edge, isOut);
            var target = Tuple.Create(node2, edge, isOut);
            List<NodeReference> nodes;
            if (!_nodes.TryGetValue(key, out nodes))
                _nodes[key] = nodes = new List<NodeReference>();

            List<Tuple<NodeReference, string, bool>> targets;
            if (!_targets.TryGetValue(node1, out targets))
                _targets[node1] = targets = new List<Tuple<NodeReference, string, bool>>();

            nodes.Add(node2);
            targets.Add(target);
        }

        internal IEnumerable<Tuple<NodeReference, string, bool>> GetTargets(NodeReference node)
        {
            List<Tuple<NodeReference, string, bool>> targets;
            if (_targets.TryGetValue(node, out targets))
                return targets;

            return new Tuple<NodeReference, string, bool>[0];
        }
    }
}
