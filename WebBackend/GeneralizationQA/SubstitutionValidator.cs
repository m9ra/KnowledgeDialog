using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.GeneralizationQA
{
    class SubstitutionValidator
    {
        /// <summary>
        /// Node which paths will be validated.
        /// </summary>
        internal readonly NodeReference Substitution;

        /// <summary>
        /// Graph against which we validate.
        /// </summary>
        internal readonly ComposedGraph Graph;

        /// <summary>
        /// We will cache results for known trace nodes.
        /// </summary>
        private Dictionary<TraceNode, bool> _resultCache = new Dictionary<TraceNode, bool>();

        internal SubstitutionValidator(NodeReference substitution, ComposedGraph graph)
        {
            Substitution = substitution;
            Graph = graph;
        }

        /// <summary>
        /// Determine whether substitution is compatible with given traceNode path.
        /// </summary>
        /// <param name="traceNode">The traceNode where path begins.</param>
        /// <returns><c>True</c> for compatible trace, <c>false</c> otherwise.</returns>
        internal bool IsCompatible(TraceNode traceNode)
        {
            var currentNode = traceNode;
            var currentLayer = new HashSet<NodeReference>();
            currentLayer.Add(Substitution);

            while (currentNode.PreviousNode != null)
            {
                if (_resultCache.ContainsKey(currentNode))
                    //we have compatibility information stored already
                    return cacheResultPath(traceNode, _resultCache[currentNode]);

                currentLayer = makeLayerTransition(currentLayer, currentNode.CurrentEdge);
                if (currentLayer.Count == 0)
                    //there is no compatible transition
                    return cacheResultPath(traceNode, false);

                currentNode = currentNode.PreviousNode;
            }

            //all transistions were compatible
            return cacheResultPath(traceNode, true);
        }

        /// <summary>
        /// Caches the result along the whole path.
        /// </summary>
        private bool cacheResultPath(TraceNode initialNode, bool cachedValue)
        {
            var currentNode = initialNode;

            while (currentNode != null)
            {
                if (_resultCache.ContainsKey(initialNode))
                    break;

                _resultCache[currentNode] = cachedValue;
                currentNode = currentNode.PreviousNode;
            }

            return cachedValue;
        }

        /// <summary>
        /// Make transition from layer to a next one along the given edge.
        /// </summary>
        private HashSet<NodeReference> makeLayerTransition(HashSet<NodeReference> layer, Edge edge)
        {
            return Graph.GetForwardTargets(layer, new[] { edge.Reversed() });
        }
    }
}
