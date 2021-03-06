﻿using System;
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
        /// Segments that will be visited.
        /// </summary>
        private readonly Queue<PathSegment> _segmentsToVisit = new Queue<PathSegment>();

        /// <summary>
        /// Maximum width for neighbours searching in graph.
        /// </summary>
        private readonly int _maxSearchWidth;

        /// <summary>
        /// Maximum depth for searching in graph.
        /// </summary>
        private readonly int _maxSearchDepth;

        /// <summary>
        /// Flag determining whether we are interested in paths with distnict edges only.
        /// </summary>
        private readonly bool _distinctEdges;

        internal PathFactory(NodeReference targetNode, ComposedGraph graph, bool distinctEdges, int maxSearchWidth, int maxSearchDepth)
        {
            StartingNode = targetNode;
            Graph = graph;
            _maxSearchDepth = maxSearchDepth;
            _distinctEdges = distinctEdges;

            _maxSearchWidth = maxSearchWidth;
            _segmentsToVisit.Enqueue(new PathSegment(null, null, StartingNode));
        }

        private void addChildren(NodeReference node, PathSegment previousSegment, ComposedGraph graph)
        {
            foreach (var edgeTuple in graph.GetNeighbours(node, _maxSearchWidth))
            {
                var edge = edgeTuple.Item1;
                var child = edgeTuple.Item2;

                if (previousSegment != null && (previousSegment.Contains(child) || (_distinctEdges && previousSegment.Contains(edge.Name))))
                    //the node has already been visited previously in the path
                    continue;

                if (previousSegment.SegmentIndex < _maxSearchDepth)
                    _segmentsToVisit.Enqueue(new PathSegment(previousSegment, edge, child));
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

            var nextSegment = _segmentsToVisit.Dequeue();

            return nextSegment;
        }

        /// <summary>
        /// Enqueues children of the segment for visiting.
        /// </summary>
        /// <param name="segment">Segment which children will be enqueued.</param>
        internal void Enqueue(PathSegment segment)
        {
            addChildren(segment.Node, segment, Graph);
        }
    }
}
