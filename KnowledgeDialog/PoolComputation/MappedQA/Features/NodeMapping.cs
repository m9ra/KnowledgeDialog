using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class NodeMapping
    {
        internal bool IsEmpty { get { return _generalizationMapping.Count == 0; } }

        /// <summary>
        /// Determine whether any node has been mapped.
        /// </summary>
        internal bool WasUsed { get; private set; }

        /// <summary>
        /// Determine whether mapping is generalizing or instantiating
        /// </summary>
        internal bool IsGeneralizeMapping;

        /// <summary>
        /// Instance nodes of the mapping.
        /// </summary>
        internal IEnumerable<NodeReference> InstanceNodes { get { return _generalizationMapping.Keys; } }

        private readonly Dictionary<NodeReference, NodeReference> _generalizationMapping = new Dictionary<NodeReference, NodeReference>();

        private readonly Dictionary<NodeReference, NodeReference> _instantiationMapping = new Dictionary<NodeReference, NodeReference>();

        private readonly ComposedGraph _graph;


        internal NodeMapping(ComposedGraph graph)
        {
            _graph = graph;
        }

        internal void SetMapping(string instanceNodeData, string generalNodeData)
        {
            var instanceNode = _graph.GetNode(instanceNodeData);
            var generalNode = _graph.GetNode(generalNodeData);

            _generalizationMapping.Add(instanceNode, generalNode);
            _instantiationMapping.Add(generalNode, instanceNode);
        }

        internal NodeReference GetMappedNode(NodeReference node)
        {
            NodeReference mappedNode;

            if (IsGeneralizeMapping)
            {
                if (!_generalizationMapping.TryGetValue(node, out mappedNode))
                    return node;
            }
            else
            {
                if (!_instantiationMapping.TryGetValue(node, out mappedNode))
                    return node;
            }

            //mapping has been found
            WasUsed = true;
            return mappedNode;
        }
    }
}
