using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    /// <summary>
    /// Knowledge graph representation composed from layers.
    /// </summary>
    public class ComposedGraph
    {
        #region Standard relations

        public static readonly string IsRelation = "is";

        public static readonly string HasFlag = "has flag";

        #endregion

        #region Standard nodes

        public static readonly string Flag = "flag";

        public static readonly string Active = "active";

        #endregion

        /// <summary>
        /// Concrete value of the is relation.
        /// TODO: make it dependent on layerbases.
        /// </summary>
        public readonly string IsEdge = "P31";

        /// <summary>
        /// Layers that are contained within the graph.
        /// </summary>
        private readonly GraphLayerBase[] _layers;

        public ComposedGraph(params GraphLayerBase[] layers)
        {
            _layers = layers.ToArray();
        }

        /// <summary>
        /// Get node associated with given data.
        /// </summary>
        /// <param name="data">Data of desired node.</param>
        /// <returns></returns>
        public NodeReference GetNode(string data)
        {
            return new NodeReference(data);
        }

        public bool HasEvidence(string data)
        {
            var node = GetNode(data);

            foreach (var layer in _layers)
            {
                if (layer.Incoming(node).Any())
                    return true;

                if (layer.Outcoming(node).Any())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determine whether given edge sequence cause twice visiting of some node.
        /// </summary>
        /// <param name="startingNodes"></param>
        /// <param name="edgePairs"></param>
        /// <returns></returns>
        public bool ContainsLoop(IEnumerable<NodeReference> startingNodes, IEnumerable<Edge> edgePairs)
        {
            var visitedNodes = new HashSet<NodeReference>(startingNodes);
            var currentLayer = new HashSet<NodeReference>(startingNodes);
            var nextLayer = new HashSet<NodeReference>();
            foreach (var edge in edgePairs)
            {
                foreach (var node in currentLayer)
                {
                    var targets = Targets(node, edge);
                    foreach (var target in targets)
                    {
                        if (!visitedNodes.Add(target))
                            //the loop has been found
                            return true;

                        nextLayer.Add(target);
                    }
                }

                var tmpXChg = currentLayer;
                currentLayer = nextLayer;
                nextLayer = tmpXChg;
                nextLayer.Clear();
            }
            return false;
        }

        /// <summary>
        /// Determine whether there is an edge between from and to nodes and has specified direction.
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="edge"></param>
        /// <param name="isOutcoming"></param>
        /// <param name="toNode"></param>
        /// <returns></returns>
        public bool HasEdge(NodeReference fromNode, string edge, bool isOutcoming, NodeReference toNode)
        {
            if (isOutcoming)
                return HasEdge(fromNode, edge, toNode);
            else
                return HasEdge(toNode, edge, fromNode);
        }

        /// <summary>
        /// Determine whether there is an edge between given nodes
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="edge"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public bool HasEdge(NodeReference node1, string edge, NodeReference node2)
        {
            foreach (var layer in _layers)
            {
                var edges = layer.Edges(node1, node2);
                if (edges.Contains(edge))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get neighbours of given fromNode that are connected by outgoing edges.
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public IEnumerable<NodeReference> OutcommingTargets(NodeReference fromNode, string edge)
        {
            foreach (var layer in _layers)
            {
                foreach (var node in layer.Outcoming(fromNode, edge))
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Get neighbours of given toNode that are connected by incomming edges.
        /// </summary>
        /// <param name="toNode"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public IEnumerable<NodeReference> IncommingTargets(NodeReference toNode, string edge)
        {
            foreach (var layer in _layers)
            {
                foreach (var node in layer.Incoming(toNode, edge))
                {
                    yield return node;
                }
            }
        }


        /// <summary>
        /// Get neighbours of given node that are connected by edge
        /// </summary>
        /// <param name="node"></param>
        /// <param name="edge"></param>
        /// <param name="isOutcomming"></param>
        /// <returns></returns>
        public IEnumerable<NodeReference> Targets(NodeReference node, Edge edge)
        {
            if (edge.IsOutcoming)
                return OutcommingTargets(node, edge.Name);
            else
                return IncommingTargets(node, edge.Name);
        }

        /// <summary>
        /// Get paths between given nodes.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="maxLength"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        public IEnumerable<KnowledgePath> GetPaths(NodeReference from, NodeReference to, int maxLength, int maxWidth)
        {
            if (from == null || to == null)
                yield break;

            var currentQueue = new Queue<PathSegment>();
            var visitedNodes = new HashSet<NodeReference>();

            var startSegment = new PathSegment(null, null, from);
            if (from.Equals(to))
            {
                yield return new KnowledgePath(startSegment);
                yield break;
            }


            //starting node
            currentQueue.Enqueue(startSegment);
            //delimiter - for counting path length
            currentQueue.Enqueue(null);

            visitedNodes.Add(from);
            visitedNodes.Add(to);

            var currentPathLength = 0;
            while (currentQueue.Count > 0 && currentPathLength < maxLength)
            {
                var currentSegment = currentQueue.Dequeue();
                if (currentSegment == null)
                {
                    ++currentPathLength;
                    //add next delimiter
                    currentQueue.Enqueue(null);
                    continue;
                }

                //test if we can get into end node
                foreach (var edge in BetweenEdges(currentSegment.Node, to))
                {
                    var segment = new PathSegment(currentSegment, edge, to);
                    yield return new KnowledgePath(segment);
                }

                //explore next children
                foreach (var childPair in GetNeighbours(currentSegment.Node, maxWidth))
                {
                    var edge = childPair.Item1;
                    var child = childPair.Item2;
                    if (!visitedNodes.Add(child))
                        //this node has already been visited
                        continue;

                    var childSegment = new PathSegment(currentSegment, edge, child);
                    currentQueue.Enqueue(childSegment);
                }
            }
        }

        /// <summary>
        /// Edges between given nodes (both side edges).
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public IEnumerable<Edge> BetweenEdges(NodeReference node1, NodeReference node2)
        {
            foreach (var layer in _layers)
            {
                foreach (var edge in layer.Edges(node1, node2))
                    yield return Edge.Outcoming(edge);

                foreach (var edge in layer.Edges(node2, node1))
                    yield return Edge.Incoming(edge);
            }
        }

        public HashSet<NodeReference> GetForwardTargets(IEnumerable<NodeReference> startingNodes, IEnumerable<Edge> path)
        {
            if (startingNodes == null)
                throw new ArgumentNullException("startingNodes");

            var result = new HashSet<NodeReference>();
            var currentLayer = startingNodes;
            foreach (var edge in path)
            {
                var nextLayer = new List<NodeReference>();
                foreach (var node in currentLayer)
                {
                    var targets = Targets(node, edge).ToArray();
                    nextLayer.AddRange(targets);
                }

                currentLayer = nextLayer;
            }

            result.UnionWith(currentLayer);
            return result;
        }

        public HashSet<NodeReference> GetForwardTargets(IEnumerable<NodeReference> startingNodes, KnowledgePath path)
        {
            return GetForwardTargets(startingNodes, path.Edges);
        }



        /// <summary>
        /// Get children of given node (incoming and outcoming).
        /// </summary>
        /// <param name="node"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        internal IEnumerable<Tuple<Edge, NodeReference>> GetNeighbours(NodeReference node, int maxWidth)
        {
            foreach (var layer in _layers)
            {
                foreach (var pair in layer.Incoming(node).Take(maxWidth))
                    yield return Tuple.Create(Edge.Incoming(pair.Key), pair.Value);

                foreach (var pair in layer.Outcoming(node).Take(maxWidth))
                    yield return Tuple.Create(Edge.Outcoming(pair.Key), pair.Value);
            }
        }

        internal ComposedGraph CreateSnapshot()
        {
            var layers = from layer in _layers select layer.Snapshot();
            return new ComposedGraph(layers.ToArray());
        }
    }
}
