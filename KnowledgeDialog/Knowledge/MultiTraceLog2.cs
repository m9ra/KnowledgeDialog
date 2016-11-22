using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    /// <summary>
    /// Efficient reimplementation of <see cref="MultiTraceLog"/>.
    /// </summary>
    public class MultiTraceLog2
    {
        public readonly TraceNode2 Root;

        public readonly IEnumerable<NodeReference> InitialNodes;

        public readonly IEnumerable<TraceNode2> TraceNodes;

        public MultiTraceLog2(IEnumerable<NodeReference> initialNodes, ComposedGraph graph, bool fullExpansion, int maxWidth, int maxDepth)
        {
            InitialNodes = initialNodes.ToArray();
            Root = new TraceNode2(InitialNodes);

            var worklist = new Queue<TraceNode2>();
            worklist.Enqueue(Root);
            var allNodes = new List<TraceNode2>();

            var start2 = DateTime.Now;
     //       Console.WriteLine("MultiTraceLog2 START");
            while (worklist.Count > 0)
            {
                var node = worklist.Dequeue();
                allNodes.Add(node);
                if (node.TraceDepth >= maxDepth || (!fullExpansion && !node.HasContinuation))
                    continue;

                //targets available from the node
                var targets = node.GetTargets(graph, maxWidth).ToArray();

                // index targets according to initial nodes and edges
                var initialNodeTargetsIndex = new Dictionary<Edge, Dictionary<NodeReference, HashSet<NodeReference>>>();
                foreach (var target in targets)
                {
                    Dictionary<NodeReference, HashSet<NodeReference>> targetIndex;
                    if (!initialNodeTargetsIndex.TryGetValue(target.Item2, out targetIndex))
                        initialNodeTargetsIndex[target.Item2] = targetIndex = new Dictionary<NodeReference, HashSet<NodeReference>>();

                    HashSet<NodeReference> edgeTargets;
                    if (!targetIndex.TryGetValue(target.Item1, out edgeTargets))
                        targetIndex[target.Item1] = edgeTargets = new HashSet<NodeReference>();

                    edgeTargets.Add(target.Item3);
                }

                //construct trace nodes
                foreach (var edgePair in initialNodeTargetsIndex)
                {
                    var traceNode = new TraceNode2(node, edgePair.Key, edgePair.Value);
                    worklist.Enqueue(traceNode);
                }
            }

 //           Console.WriteLine("MultiTraceLog2 {0}s", (DateTime.Now - start2).TotalSeconds);
            TraceNodes = allNodes;
        }
    }

    public class TraceNode2
    {
        /// <summary>
        /// Depth of the path from initial node to current node.
        /// </summary>
        public readonly int TraceDepth;

        /// <summary>
        /// Parent of this node.
        /// </summary>
        public readonly TraceNode2 PreviousNode;

        /// <summary>
        /// Edge which was used for comming into the node from its parent
        /// </summary>
        public readonly Edge CurrentEdge;

        /// <summary>
        /// Initial nodes which are trace compatible with current trace.
        /// </summary>
        public IEnumerable<NodeReference> CompatibleInitialNodes { get { return _initialToTargetIndex.Keys; } }

        /// <summary>
        /// Nodes that were reached at current trace node.
        /// </summary>
        public IEnumerable<NodeReference> CurrentNodes { get { return _initialToTargetIndex.Values.SelectMany(v => v).Distinct(); } }

        /// <summary>
        /// Determine whether it makes sence to prolong the trace further.
        /// </summary>
        public bool HasContinuation { get { return _initialToTargetIndex.Count > 1 && CurrentNodes.Skip(1).Any(); } }

        /// <summary>
        /// Path from traces root.
        /// </summary>
        public IEnumerable<Edge> Path
        {
            get
            {
                var path = new List<Edge>();
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


        /// <summary>
        /// Index of targets from initial nodes.
        /// </summary>
        private readonly Dictionary<NodeReference, HashSet<NodeReference>> _initialToTargetIndex;

        internal TraceNode2(IEnumerable<NodeReference> initialNodes)
        {
            //we are at trace begining
            TraceDepth = 0;

            //empty index initialization
            _initialToTargetIndex = new Dictionary<NodeReference, HashSet<NodeReference>>();
            foreach (var node in initialNodes)
            {
                _initialToTargetIndex[node] = new HashSet<NodeReference>(new[] { node });
            }
        }

        internal TraceNode2(TraceNode2 parent, Edge edge, Dictionary<NodeReference, HashSet<NodeReference>> initialToTargetsIndex)
        {
            PreviousNode = parent;
            TraceDepth = parent.TraceDepth + 1;
            CurrentEdge = edge;
            _initialToTargetIndex = initialToTargetsIndex;
        }

        internal IEnumerable<Tuple<NodeReference, Edge, NodeReference>> GetTargets(ComposedGraph graph, int maxWidth)
        {
            foreach (var initialNodePair in _initialToTargetIndex)
            {
                foreach (var currentNode in initialNodePair.Value)
                {
                    foreach (var target in graph.GetNeighbours(currentNode, maxWidth))
                    {
                        yield return Tuple.Create(initialNodePair.Key, target.Item1, target.Item2);
                    }
                }
            }
        }
    }
}
