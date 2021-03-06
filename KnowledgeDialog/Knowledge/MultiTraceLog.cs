﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.Knowledge
{
    public class MultiTraceLog
    {
        public static readonly int Width = 200;

        public static readonly int MaxPathLength = 1;

        /// <summary>
        /// Trace nodes where whole trace ends. (All traces for the route are joined here or no other way for trace is possible.)
        /// </summary>
        public readonly IEnumerable<TraceNode> TraceNodes;

        /// <summary>
        /// Root of all the trace nodes.
        /// </summary>
        public readonly TraceNode Root;

        /// <summary>
        /// Nodes traced within the log.
        /// </summary>
        public readonly IEnumerable<NodeReference> NodeBatch;

        /// <summary>
        /// Create trace log for the given batch of nodes.
        /// </summary>
        /// <param name="nodeBatch">Nodes that will be traced.</param>
        public MultiTraceLog(IEnumerable<NodeReference> nodeBatch, ComposedGraph graph)
        {
            NodeBatch = nodeBatch.ToArray();
            Root = new TraceNode(nodeBatch);
            var worklist = new Queue<TraceNode>();
            var allNodes = new List<TraceNode>();
            worklist.Enqueue(Root);
            while (worklist.Count > 0)
            {
                var currentNode = worklist.Dequeue();
                allNodes.Add(currentNode);
                if (!currentNode.HasContinuation || currentNode.Path.Count() >= MaxPathLength)
                    //current node cannot be extended
                    continue;

                //extend trace node according to all edges
                var edges = getEdges(currentNode, graph);
               // Console.WriteLine("M"+edges.Count());
                foreach (var edge in edges)
                {
                    if (edge.Inverse().Equals(currentNode.CurrentEdge))
                        //we dont want to go back
                        continue;
                    var nextNode = new TraceNode(currentNode, edge, graph);
                    worklist.Enqueue(nextNode);
                }
            }

            TraceNodes = allNodes;
        }

        private IEnumerable<Edge> getEdges(TraceNode node, ComposedGraph graph)
        {
            var edges = new HashSet<Edge>();
            foreach (var currentNode in node.CurrentNodes)
            {
                var neighbours = graph.GetNeighbours(currentNode, Width).ToArray();
                foreach (var neighbour in neighbours.ToArray())
                {
                    edges.Add(neighbour.Item1);
                }
            }

            return edges;
        }

        public IEnumerable<TraceNode> CreateAllTraces(int pathLengthLimit, ComposedGraph graph)
        {
            var worklist = new Queue<TraceNode>();
            var allNodes = new List<TraceNode>();
            worklist.Enqueue(Root);
            while (worklist.Count > 0)
            {
                var currentNode = worklist.Dequeue();
                allNodes.Add(currentNode);
                if (currentNode.Path.Count() >= pathLengthLimit)
                    //current node cannot be extended
                    continue;

                //extend trace node according to all edges
                var edges = getEdges(currentNode, graph);
                //Console.WriteLine("A" + edges.Count());
                foreach (var edge in edges)
                {
                    if (edge.Inverse().Equals(currentNode.CurrentEdge))
                        //we dont want to go back
                        continue;
                    var nextNode = new TraceNode(currentNode, edge, graph);
                    worklist.Enqueue(nextNode);
                }
            }

            return allNodes;
        }
    }

    public class TraceNode
    {

        public IEnumerable<Trace> Traces { get { return _traceIndex.Values.ToArray(); } }

        public IEnumerable<NodeReference> CompatibleInitialNodes { get { return _traceIndex.Values.SelectMany(t => t.InitialNodes).Distinct().ToArray(); } }

        public IEnumerable<NodeReference> CurrentNodes { get { return _traceIndex.Keys.ToArray(); } }

        public readonly bool HasContinuation;

        /// <summary>
        /// Node from which current node was accessed via the <see cref="CurrentEdge"/>.
        /// </summary>
        public readonly TraceNode PreviousNode;

        /// <summary>
        /// Edge that was used for creating of current node
        /// </summary>
        public readonly Edge CurrentEdge;

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

        protected readonly HashSet<NodeReference> VisitedNodes = new HashSet<NodeReference>();
         
        private readonly Dictionary<NodeReference, Trace> _traceIndex = new Dictionary<NodeReference, Trace>();

        internal TraceNode(IEnumerable<NodeReference> initialNodes)
        {
            foreach (var node in initialNodes)
            {
                if (node == null)
                    throw new NullReferenceException("node cannot be null");

                var trace = new Trace(node);
                _traceIndex[node] = trace;
            }

            VisitedNodes.UnionWith(initialNodes);

            //if there is more than one node, we could try to merge them
            HasContinuation = initialNodes.Skip(1).Any();
        }

        internal TraceNode(TraceNode previousNode, Edge edge, ComposedGraph graph)
        {
            if (previousNode == null)
                throw new ArgumentNullException("previousNode");

            if (edge == null)
                throw new ArgumentNullException("edge");

            PreviousNode = previousNode;
            CurrentEdge = edge;
            VisitedNodes.UnionWith(previousNode.VisitedNodes);

            var startTime = DateTime.Now;
            var currentlyVisitedNodes = new HashSet<NodeReference>();

            var traceTargetIndex = new Dictionary<NodeReference, List<Trace>>();
            //fill index with previous nodes for each trace target
            foreach (var node in previousNode.CurrentNodes)
            {
                var previousTrace = previousNode.GetTrace(node);
                //check whether we still need to trace the previousTrace 
                //if it contains all input nodes - we don't need to trace it further


                foreach (var target in graph.Targets(node, edge))
                {
                    if (VisitedNodes.Contains(target))
                        continue;

                    currentlyVisitedNodes.Add(target);
                    List<Trace> traces;
                    if (!traceTargetIndex.TryGetValue(target, out traces))
                        traceTargetIndex[target] = traces = new List<Trace>();

                    traces.Add(previousTrace);
                }
            }
            VisitedNodes.UnionWith(currentlyVisitedNodes);

            var inputInitialNodes = new HashSet<NodeReference>();
            //merge traces that points to same node and check saturation of nodes
            foreach (var pair in traceTargetIndex)
            {
                var trace = new Trace(pair.Key, pair.Value.Distinct());
                _traceIndex.Add(pair.Key, trace);
                inputInitialNodes.UnionWith(trace.InitialNodes);
            }

            //If there is node, that is not saturated - we will trace path further (because it can add more information)
            var hasNonSaturatedTrace = _traceIndex.Any(p => p.Value.InitialNodes.Count() != inputInitialNodes.Count);
            HasContinuation = hasNonSaturatedTrace && Path.Count() < MultiTraceLog.MaxPathLength;

            var endTime = DateTime.Now;
            var duration = (endTime - startTime).TotalSeconds;
          //  Console.WriteLine("Tracenode creation {0:0.000}s", duration);
          //  Console.WriteLine("TraceNode {0},{1}", VisitedNodes.Count, _traceIndex.Count);
        }

        internal Trace GetTrace(NodeReference node)
        {
            return _traceIndex[node];
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            ConsoleServices.FillWithPath(builder, Path);
            return "[TraceNode]" + builder.ToString();
        }

        public IEnumerable<Edge> GetPathFromRoot()
        {
            var path = new List<Edge>();
            var currentNode = this;
            while (currentNode.CurrentEdge != null)
            {
                path.Add(currentNode.CurrentEdge);
                currentNode = currentNode.PreviousNode;
            }
            path.Reverse();
            return path;
        }

        public IEnumerable<Edge> GetPathToRoot()
        {
            var pathFromRoot = GetPathFromRoot();
            return pathFromRoot.Reverse().Select(e => e.Reversed());
        }
    }

    public class Trace
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
