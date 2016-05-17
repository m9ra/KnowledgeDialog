using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    public class KnowledgePath
    {
        /// <summary>
        /// Last segment of path.
        /// </summary>
        private readonly PathSegment _lastSegment;

        /// <summary>
        /// Nodes contained in current path.
        /// </summary>
        private readonly List<NodeReference> _nodes = new List<NodeReference>();

        /// <summary>
        /// Edges contained in current path.
        /// </summary>
        private readonly List<Edge> _edges = new List<Edge>();

        /// <summary>
        /// Length of current path.
        /// </summary>
        public int Length { get { return _edges.Count; } }

        /// <summary>
        /// Nodes contained in current path.
        /// </summary>
        public IEnumerable<NodeReference> Nodes { get { return _nodes; } }

        /// <summary>
        /// Edges of current path.
        /// </summary>
        public IEnumerable<Edge> Edges { get { return _edges; } }

        /// <summary>
        /// Creates path from sequence of path segments in context of given graph.
        /// </summary>
        /// <param name="lastSegment"></param>
        internal KnowledgePath(PathSegment lastSegment)
        {
            _lastSegment = lastSegment;

            var previousSegment = _lastSegment;
            //add first node that has no outgoing edge
            _nodes.Add(previousSegment.Node);

            var currentSegment = previousSegment.PreviousSegment;
            while (currentSegment != null)
            {
                var currentNode = currentSegment.Node;

                _nodes.Add(currentNode);
                _edges.Add(previousSegment.Edge);

                previousSegment = currentSegment;
                currentSegment = currentSegment.PreviousSegment;
            }

            _nodes.Reverse();
            _edges.Reverse();
        }

        private KnowledgePath(IEnumerable<NodeReference> nodes, IEnumerable<Edge> edges)
        {
            _nodes.AddRange(nodes);
            _edges.AddRange(edges);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = new StringBuilder();

            for (var i = 0; i < _edges.Count; ++i)
            {
                var node = _nodes[i];
                var edge = _edges[i];
                var isOutcoming = edge.IsOutcoming;

                result.Append(node.Data);
                result.Append(isOutcoming ? " --" : " <-");
                result.Append(edge);
                result.Append(isOutcoming ? "-> " : "-- ");
            }

            result.Append(_nodes[_nodes.Count - 1].Data);

            return result.ToString();
        }

        /// <summary>
        /// Get node at given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal NodeReference Node(int index)
        {
            return _nodes[index];
        }

        /// <summary>
        /// Get edge on given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Edge GetEdge(int index)
        {
            return _edges[index];
        }


        /// <summary>
        /// Create new path by prepending given node and edge to current path.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="edge"></param>
        /// <param name="isOutcome"></param>
        /// <returns></returns>
        internal KnowledgePath PrependBy(NodeReference node, Edge edge)
        {
            return new KnowledgePath(
                new[] { node }.Concat(_nodes),
                new[] { edge }.Concat(_edges));
        }

        internal KnowledgePath TakeEnding(int startingOffset)
        {
            return new KnowledgePath(
                _nodes.Skip(startingOffset),
                _edges.Skip(startingOffset));
        }

        internal bool HasSameEdgesAs(KnowledgePath other)
        {
            if (Length != other.Length)
                return false;

            for (var i = 0; i < Length; ++i)
            {
                if (GetEdge(i) != other.GetEdge(i))
                    return false;
            }

            return true;
        }

        public IEnumerable<Edge> ReverseOrderedEdges
        {
            get
            {
                return Edges.Reverse();
            }
        }

        public IEnumerable<Edge> ReverseOrderedInverseEdges
        {
            get
            {
                return Edges.Select(e => e.Inverse());
            }
        }
    }
}
