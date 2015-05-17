using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class NodesSubstitution
    {
        internal readonly NodesEnumeration OriginalNodes;

        internal readonly NodesEnumeration SubstitutedNodes;

        internal int NodeCount { get { return OriginalNodes.Count; } }

        public NodesSubstitution(NodesEnumeration originalNodes, Dictionary<NodeReference, NodeReference> substitutions)
        {
            OriginalNodes = originalNodes;
            var orderedSubstitutions = new List<NodeReference>(NodeCount);
            foreach (var node in OriginalNodes)
            {
                orderedSubstitutions.Add(substitutions[node]);
            }

            SubstitutedNodes = new NodesEnumeration(orderedSubstitutions);
        }

        internal NodeReference GetOriginalNode(int nodeIndex)
        {
            return OriginalNodes.GetNode(nodeIndex);
        }

        internal NodeReference GetSubstitution(int nodeIndex)
        {
            return SubstitutedNodes.GetNode(nodeIndex);
        }

        internal bool TryGetValue(NodeReference patternNode, out NodeReference substitution)
        {
            for (var i = 0; i < OriginalNodes.Count; ++i)
            {
                var originalNode = OriginalNodes.GetNode(i);
                if (originalNode.Equals(patternNode))
                {
                    substitution = SubstitutedNodes.GetNode(i);
                    return true;
                }
            }
            substitution = null;
            return false;
        }
    }
}
