using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    class PathSegment
    {
        public readonly PathSegment PreviousSegment;

        public readonly NodeReference Node;

        public readonly Edge Edge;

        public readonly int SegmentIndex;

        public PathSegment(PathSegment previousSegment, Edge edge, NodeReference toNode)
        {
            if (previousSegment != null)
                SegmentIndex += previousSegment.SegmentIndex + 1;

            PreviousSegment = previousSegment;
            Edge = edge;
            Node = toNode;
        }

        /// <summary>
        /// Determine whether node is contained in currrent or previous segments.
        /// </summary>
        /// <param name="node">Tested node.</param>
        /// <returns><c>true</c> whether node is contained, <c>false</c> otherwise.</returns>
        internal bool Contains(NodeReference node)
        {
            if (Node.Equals(node))
                return true;

            if (PreviousSegment == null)
                return false;

            return PreviousSegment.Contains(node);
        }

        /// <summary>
        /// Determine whether edge is contained in currrent or previous segments.
        /// </summary>
        /// <param name="edgeName">Tested edge.</param>
        /// <returns><c>true</c> whether edge is contained, <c>false</c> otherwise.</returns>
        internal bool Contains(string edgeName)
        {
            if (Edge.Name == edgeName)
                return true;

            if (PreviousSegment == null)
                return false;

            return PreviousSegment.Contains(edgeName);
        }

        internal IEnumerable<Edge> GetEdges()
        {
            var currentSegment = this;
            while (currentSegment != null)
            {
                if (currentSegment.Edge != null)
                    yield return currentSegment.Edge;

                currentSegment = currentSegment.PreviousSegment;
            }
        }

        internal IEnumerable<Edge> GetReversedEdges()
        {
            return GetEdges().Reverse();
        }

        internal IEnumerable<Edge> GetInvertedEdges()
        {
            var currentSegment = this;
            while (currentSegment != null)
            {
                if (currentSegment.Edge != null)
                    yield return currentSegment.Edge;

                currentSegment = currentSegment.PreviousSegment;
            }
        }

        internal IEnumerable<Edge> GetReversedInvertedEdges()
        {
            return GetInvertedEdges().Reverse();
        }
    }
}
