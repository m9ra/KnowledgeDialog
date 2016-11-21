using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.GeneralizationQA
{
    class PathSubstitution
    {
        /// <summary>
        /// Nodes that were original start points of the substitution.
        /// </summary>
        public IEnumerable<NodeReference> CompatibleInitialNodes { get { return OriginalTrace.CompatibleInitialNodes; } }

        /// <summary>
        /// Node substitution.
        /// </summary>
        public readonly NodeReference Substitution;

        /// <summary>
        /// Trace which is substituted by the substitution.
        /// </summary>
        public readonly TraceNode2 OriginalTrace;

        /// <summary>
        /// How much evidence for the path we have.
        /// </summary>
        public readonly double Rank;

        internal PathSubstitution(NodeReference substitution, TraceNode2 originalTrace, double rank = double.NaN)
        {
            Substitution = substitution;
            OriginalTrace = originalTrace;

            if (double.IsNaN(rank))
            {
                Rank = CompatibleInitialNodes.Count();
            }
            else
            {
                Rank = rank;
            }
        }

        internal PathSubstitution Reranked(double confidence)
        {
            return new PathSubstitution(Substitution, OriginalTrace, confidence);
        }

        internal IEnumerable<NodeReference> FindTargets(ComposedGraph graph)
        {
            var path = OriginalTrace.Path.ToArray();
            return graph.GetForwardTargets(new[] { Substitution }, path).ToArray();
        }
    }
}
