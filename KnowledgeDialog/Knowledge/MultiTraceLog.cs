using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    class MultiTraceLog
    {
        public static readonly int Width = 20;

        /// <summary>
        /// Trace nodes where whole trace ends. (All traces for the route are joined here or no other way for trace is possible.)
        /// </summary>
        public readonly IEnumerable<TraceNode> TraceNodes;

        /// <summary>
        /// Create trace log for the given batch of nodes.
        /// </summary>
        /// <param name="nodeBatch">Nodes that will be traced.</param>
        public MultiTraceLog(IEnumerable<NodeReference> nodeBatch, ComposedGraph graph)
        {
            var rootNode = new TraceNode(nodeBatch);
            var worklist = new Queue<TraceNode>();
            var allNodes = new List<TraceNode>();
            worklist.Enqueue(rootNode);
            while (worklist.Count > 0)
            {
                var currentNode = worklist.Dequeue();
                allNodes.Add(currentNode);
                if (!currentNode.HasContinuation)
                    //current node cannot be extended
                    continue;

                //extend trace node according to all edges
                var edges = getEdges(currentNode, graph);
                foreach (var edge in edges)
                {
                    var nextNode = new TraceNode(currentNode, edge, graph);
                    worklist.Enqueue(nextNode);
                }
            }

            TraceNodes = allNodes;
        }

        private IEnumerable<Tuple<string, bool>> getEdges(TraceNode node, ComposedGraph graph)
        {
            var edges = new HashSet<Tuple<string, bool>>();
            foreach (var currentNode in node.CurrentNodes)
            {
                var neighbours = graph.GetNeighbours(currentNode, Width).ToArray();
                foreach (var neighbour in neighbours.ToArray())
                {
                    edges.Add(Tuple.Create(neighbour.Item1, neighbour.Item2));
                }
            }

            return edges;
        }
    }

    class TraceNode
    {

        internal IEnumerable<Trace> Traces { get; private set; }

        internal IEnumerable<NodeReference> CurrentNodes { get { return _traceIndex.Keys; } }

        internal bool HasContinuation { get { return _traceIndex.Count > 1; } }

        internal IEnumerable<Tuple<string, bool>> Path
        {
            get
            {
                var path = new List<Tuple<string, bool>>();
                var currentNode = this;
                while (currentNode != null && currentNode.CurrentEdge != null)
                {
                    path.Add(currentNode.CurrentEdge);
                    currentNode = currentNode.PreviousNode;
                }

                path.Reverse();

                return path;
            }
        }

        internal readonly TraceNode PreviousNode;

        protected readonly HashSet<NodeReference> VisitedNodes = new HashSet<NodeReference>();

        private readonly Dictionary<NodeReference, List<Trace>> _traceIndex = new Dictionary<NodeReference, List<Trace>>();


        /// <summary>
        /// Edge that was used for creating of current node
        /// </summary>
        private readonly Tuple<string, bool> CurrentEdge;

        internal TraceNode(IEnumerable<NodeReference> initialNodes)
        {
            var allTraces = new List<Trace>();
            foreach (var node in initialNodes)
            {
                if (node == null)
                    throw new NullReferenceException("node cannot be null");

                var trace = new Trace(node);
                allTraces.Add(trace);

                var nodeTraces = new List<Trace>();
                nodeTraces.Add(trace);

                _traceIndex[node] = nodeTraces;
            }

            VisitedNodes.UnionWith(initialNodes);
            Traces = allTraces;
        }

        internal TraceNode(TraceNode previousNode, Tuple<string, bool> edge, ComposedGraph graph)
        {
            if (previousNode == null)
                throw new ArgumentNullException("previousNode");

            if (edge == null)
                throw new ArgumentNullException("edge");

            PreviousNode = previousNode;
            CurrentEdge = edge;
            VisitedNodes.UnionWith(previousNode.VisitedNodes);

            var currentlyVisitedNodes = new HashSet<NodeReference>();
            var allTraces = new List<Trace>();

            var traceTargetIndex = new Dictionary<NodeReference, HashSet<Trace>>();
            //fill index with previous nodes for each trace target
            foreach (var node in previousNode.CurrentNodes)
            {                
                var previousTraces = previousNode.GetTraces(node);
                foreach (var target in graph.Targets(node, edge.Item1, edge.Item2))
                {
                    if (VisitedNodes.Contains(target))
                        continue;

                    currentlyVisitedNodes.Add(target);                    
                    HashSet<Trace> traces;
                    if (!traceTargetIndex.TryGetValue(target, out traces))
                        traceTargetIndex[target] = traces = new HashSet<Trace>();

                    traces.UnionWith(previousTraces);
                }
            }

            //create traces from index
            foreach (var pair in traceTargetIndex)
            {
                var trace = new Trace(pair.Key, pair.Value);
                allTraces.Add(trace);
            }

            VisitedNodes.UnionWith(currentlyVisitedNodes);
            Traces = allTraces;
        }

        internal IEnumerable<Trace> GetTraces(NodeReference node)
        {
            return _traceIndex[node];
        }
    }

    class Trace
    {
        /// <summary>
        /// Initial nodes of current trace.
        /// </summary>
        public readonly IEnumerable<NodeReference> InitialNodes;

        /// <summary>
        /// Node where current trace ends.
        /// </summary>
        public readonly NodeReference CurrentNode;

        /// <summary>
        /// Traces that preceeds current trace.
        /// </summary>
        public readonly IEnumerable<Trace> PreviousTraces;

        internal Trace(NodeReference initialNode)
        {
            if (initialNode == null)
                throw new ArgumentNullException("initialNode");

            CurrentNode = initialNode;

            InitialNodes = new[] { CurrentNode };
            PreviousTraces = new Trace[0];
        }

        internal Trace(NodeReference currentNode, IEnumerable<Trace> previousTraces)
        {
            CurrentNode = currentNode;
            PreviousTraces = previousTraces;

            var initialNodes = new HashSet<NodeReference>();
            foreach (var trace in previousTraces)
            {
                initialNodes.UnionWith(trace.InitialNodes);
            }

            InitialNodes = initialNodes.ToArray();
        }
    }
}
