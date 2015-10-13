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

        public readonly string Edge;

        public readonly bool IsOutcoming;

        public PathSegment(PathSegment previousSegment, string edge, bool isOutcoming, NodeReference toNode)
        {
            PreviousSegment = previousSegment;
            Edge = edge;
            Node = toNode;
            IsOutcoming = isOutcoming;
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

        internal IEnumerable<Tuple<string, bool>> GetEdges()
        {
            var currentSegment = this;
            while (currentSegment != null)
            {
                if (currentSegment.Edge != null)
                    yield return Tuple.Create(currentSegment.Edge, currentSegment.IsOutcoming);

                currentSegment = currentSegment.PreviousSegment;
            }
        }

        internal IEnumerable<Tuple<string, bool>> GetReversedEdges()
        {
            return GetInvertedEdges().Reverse();
        }

        internal IEnumerable<Tuple<string, bool>> GetInvertedEdges()
        {
            var currentSegment = this;
            while (currentSegment != null)
            {
                if (currentSegment.Edge != null)
                    yield return Tuple.Create(currentSegment.Edge, !currentSegment.IsOutcoming);

                currentSegment = currentSegment.PreviousSegment;
            }
        }
    }
}
