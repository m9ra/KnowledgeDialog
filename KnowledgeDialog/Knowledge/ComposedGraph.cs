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
        public IEnumerable<NodeReference> Targets(NodeReference node, string edge, bool isOutcomming)
        {
            if (isOutcomming)
                return OutcommingTargets(node, edge);
            else
                return IncommingTargets(node, edge);
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

            var startSegment = new PathSegment(null, null, false, from);
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
                    var segment = new PathSegment(currentSegment, edge.Item1, edge.Item2, to);
                    yield return new KnowledgePath(segment);
                }

                //explore next children
                foreach (var childPair in GetNeighbours(currentSegment.Node, maxWidth))
                {
                    var edge = childPair.Item1;
                    var isOut = childPair.Item2;
                    var child = childPair.Item3;
                    if (!visitedNodes.Add(child))
                        //this node has already been visited
                        continue;

                    var childSegment = new PathSegment(currentSegment, edge, isOut, child);
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
        public IEnumerable<Tuple<string, bool>> BetweenEdges(NodeReference node1, NodeReference node2)
        {
            foreach (var layer in _layers)
            {
                foreach (var edge in layer.Edges(node1, node2))
                    yield return Tuple.Create(edge, true);

                foreach (var edge in layer.Edges(node2, node1))
                    yield return Tuple.Create(edge, false);
            }
        }

        public HashSet<NodeReference> GetForwardTargets(IEnumerable<NodeReference> startingNodes, IEnumerable<Tuple<string, bool>> path)
        {
            if (startingNodes == null)
                throw new ArgumentNullException("startingNodes");

            var result = new HashSet<NodeReference>();
            var currentLayer = startingNodes;
            foreach (var part in path)
            {
                var nextLayer = new List<NodeReference>();
                foreach (var node in currentLayer)
                {
                    var targets = Targets(node, part.Item1,part.Item2).ToArray();
                    nextLayer.AddRange(targets);
                }

                currentLayer = nextLayer;
            }

            result.UnionWith(currentLayer);
            return result;
        }

        public HashSet<NodeReference> GetForwardTargets(IEnumerable<NodeReference> startingNodes, KnowledgePath path)
        {
            return GetForwardTargets(startingNodes, path.CompleteEdges);
        }



        /// <summary>
        /// Get children of given node (incoming and outcoming).
        /// </summary>
        /// <param name="node"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        internal IEnumerable<Tuple<string, bool, NodeReference>> GetNeighbours(NodeReference node, int maxWidth)
        {
            foreach (var layer in _layers)
            {
                foreach (var pair in layer.Incoming(node).Take(maxWidth))
                    yield return Tuple.Create(pair.Key, false, pair.Value);

                foreach (var pair in layer.Outcoming(node).Take(maxWidth))
                    yield return Tuple.Create(pair.Key, true, pair.Value);
            }
        }

        internal ComposedGraph CreateSnapshot()
        {
            var layers = from layer in _layers select layer.Snapshot();
            return new ComposedGraph(layers.ToArray());
        }
    }
}
