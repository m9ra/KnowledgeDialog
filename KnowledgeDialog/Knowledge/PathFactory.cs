using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    class PathFactory
    {
        /// <summary>
        /// Graph used for data retreiving.
        /// </summary>
        internal readonly ComposedGraph Graph;

        /// <summary>
        /// Node where searching starts.
        /// </summary>
        internal readonly NodeReference StartingNode;

        /// <summary>
        /// Determine whether there is next path in the factory.
        /// </summary>
        internal bool HasNextPath { get { return _segmentsToVisit.Count > 0; } }

        /// <summary>
        /// Stack of segments that will be visited.
        /// </summary>
        private readonly Stack<PathSegment> _segmentsToVisit = new Stack<PathSegment>();

        /// <summary>
        /// Maximum width for neighbours searching in graph.
        /// </summary>
        private readonly int _maxSearchWidth;

        /// <summary>
        /// Maximum depth for searching in graph.
        /// </summary>
        private readonly int _maxSearchDepth;

        internal PathFactory(NodeReference targetNode, ComposedGraph graph, int maxSearchWidth, int maxSearchDepth)
        {
            StartingNode = targetNode;
            Graph = graph;
            _maxSearchDepth = maxSearchDepth;

            _maxSearchWidth = maxSearchWidth;
            _segmentsToVisit.Push(new PathSegment(null, null, false, StartingNode));
        }

        private void addChildren(NodeReference node, PathSegment previousSegment, ComposedGraph graph)
        {
            foreach (var edgeTuple in graph.GetNeighbours(node, _maxSearchWidth))
            {
                var edge = edgeTuple.Item1;
                var isOutcomming = edgeTuple.Item2;
                var child = edgeTuple.Item3;

                if (previousSegment != null && previousSegment.Contains(child) )
                    //the node has already been visited previously in the path
                    continue;

                if (previousSegment.SegmentIndex < _maxSearchDepth)
                    _segmentsToVisit.Push(new PathSegment(previousSegment, edge, isOutcomming, child));
            }
        }

        /// <summary>
        /// Gets ending segment of next path.
        /// </summary>
        /// <returns>The next segment.</returns>
        internal PathSegment GetNextSegment()
        {
            if (_segmentsToVisit.Count == 0)
                return null;

            var nextSegment = _segmentsToVisit.Pop();
            addChildren(nextSegment.Node, nextSegment, Graph);

            return nextSegment;
        }

    }
}
