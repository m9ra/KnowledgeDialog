using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    public class ExplicitLayer : GraphLayerBase
    {
        /// <summary>
        /// Index of edges in form of from-edge-to.
        /// </summary>
        private readonly Dictionary<NodeReference, Dictionary<string, List<NodeReference>>> _outEdges;

        /// <summary>
        /// Index of edges in form of to-edge-from.
        /// </summary>
        private readonly Dictionary<NodeReference, Dictionary<string, List<NodeReference>>> _inEdges;


        public ExplicitLayer()
            : this(new Dictionary<NodeReference, Dictionary<string, List<NodeReference>>>(), new Dictionary<NodeReference, Dictionary<string, List<NodeReference>>>())
        {
        }

        private ExplicitLayer(Dictionary<NodeReference, Dictionary<string, List<NodeReference>>> outEdges, Dictionary<NodeReference, Dictionary<string, List<NodeReference>>> inEdges)
        {
            _outEdges = new Dictionary<NodeReference, Dictionary<string, List<NodeReference>>>(outEdges);
            _inEdges = new Dictionary<NodeReference, Dictionary<string, List<NodeReference>>>(inEdges);
        }

        /// <summary>
        /// Add edge to the layer.
        /// </summary>
        /// <param name="from">From node.</param>
        /// <param name="edge">Edge between from and to nodes.</param>
        /// <param name="to">To node.</param>
        public void AddEdge(NodeReference from, string edge, NodeReference to)
        {
            addEdge(from, edge, to, _outEdges);
            addEdge(to, edge, from, _inEdges);
        }

        /// <summary>
        /// Removes edge from layer.
        /// </summary>
        /// <param name="from">From node.</param>
        /// <param name="edge">Edge between from and to nodes.</param>
        /// <param name="to">To node.</param>
        public void RemoveEdge(NodeReference from, string edge, NodeReference to)
        {
            Dictionary<string, List<NodeReference>> outEdgeNodes;
            Dictionary<string, List<NodeReference>> inEdgeNodes;
            List<NodeReference> outNodes;
            List<NodeReference> inNodes;
            if (
                _outEdges.TryGetValue(from, out outEdgeNodes) &&
                _inEdges.TryGetValue(to, out inEdgeNodes) &&
                outEdgeNodes.TryGetValue(edge, out outNodes) &&
                inEdgeNodes.TryGetValue(edge, out inNodes)
                )
            {
                //edge will be removed
                outNodes.Remove(to);
                inNodes.Remove(from);
                if (outNodes.Count > 0 || inNodes.Count > 0)
                    return;

                outEdgeNodes.Remove(edge);
                inEdgeNodes.Remove(edge);

                if (outEdgeNodes.Count > 0 || inEdgeNodes.Count > 0)
                    return;

                _outEdges.Remove(from);
                _inEdges.Remove(to);
            }
        }

        /// <inheritdoc/>
        protected internal override IEnumerable<string> Edges(NodeReference from, NodeReference to)
        {
            Dictionary<string, List<NodeReference>> targets;
            if (!_outEdges.TryGetValue(from, out targets))
                yield break;

            //enumerate all edge connections
            foreach (var targetPair in targets)
            {
                if (targetPair.Value.Contains(to))
                    yield return targetPair.Key;
            }
        }

        /// <inheritdoc/>
        protected internal override IEnumerable<KeyValuePair<string, NodeReference>> Incoming(NodeReference node)
        {
            return getEdges(node, _inEdges);

        }

        /// <inheritdoc/>
        protected internal override IEnumerable<KeyValuePair<string, NodeReference>> Outcoming(NodeReference node)
        {
            return getEdges(node, _outEdges);
        }

        /// <inheritdoc/>
        protected internal override IEnumerable<NodeReference> Outcoming(NodeReference fromNode, string edge)
        {
            return getTargets(fromNode, edge, _outEdges);
        }

        /// <inheritdoc/>
        protected internal override IEnumerable<NodeReference> Incoming(NodeReference toNode, string edge)
        {
            return getTargets(toNode, edge, _inEdges);
        }

        /// <inheritdoc/>
        internal override GraphLayerBase Snapshot()
        {
            return new ExplicitLayer(_outEdges, _inEdges);
        }

        private IEnumerable<NodeReference> getTargets(NodeReference node, string edge, Dictionary<NodeReference, Dictionary<string, List<NodeReference>>> edges)
        {
            Dictionary<string, List<NodeReference>> targetCandidates;
            List<NodeReference> targets;
            if (
                edges.TryGetValue(node, out targetCandidates) &&
                targetCandidates.TryGetValue(edge, out targets)
                )
                return targets;

            return Enumerable.Empty<NodeReference>();
        }

        private IEnumerable<KeyValuePair<string, NodeReference>> getEdges(NodeReference node, Dictionary<NodeReference, Dictionary<string, List<NodeReference>>> edges)
        {
            Dictionary<string, List<NodeReference>> targets;
            if (!edges.TryGetValue(node, out targets))
                yield break;

            //enumerate all incoming edges
            foreach (var targetPair in targets)
            {
                foreach (var target in targetPair.Value)
                {
                    yield return new KeyValuePair<string, NodeReference>(targetPair.Key, target);
                }
            }
        }

        private void addEdge(NodeReference from, string edge, NodeReference to, Dictionary<NodeReference, Dictionary<string, List<NodeReference>>> edges)
        {
            Dictionary<string, List<NodeReference>> edgeNodes;
            if (
                !edges.TryGetValue(from, out edgeNodes)
                )
            {
                edges[from] = edgeNodes = new Dictionary<string, List<NodeReference>>();
            }

            List<NodeReference> nodes;
            if (
                !edgeNodes.TryGetValue(edge, out nodes)
                )
            {
                edgeNodes[edge] = nodes = new List<NodeReference>();
            }

            nodes.Add(to);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var edges in _outEdges)
            {
                var from = edges.Key;
                foreach (var target in edges.Value)
                {
                    foreach (var to in target.Value)
                    {
                        builder.AppendLine(string.Format("{0}--{1}-->{2}", from, target.Key, to));
                    }
                }
            }
            return builder.ToString();
        }
    }
}
