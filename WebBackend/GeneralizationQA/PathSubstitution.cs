﻿using System;
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
        public IEnumerable<NodeReference> OriginalNodes { get { return OriginalTrace.CurrentNodes; } }

        /// <summary>
        /// Node substitution.
        /// </summary>
        public readonly NodeReference Substitution;

        /// <summary>
        /// Trace which is substituted by the substitution.
        /// </summary>
        public readonly TraceNode OriginalTrace;

        /// <summary>
        /// How much evidence for the path we have.
        /// </summary>
        public readonly double Rank;

        internal PathSubstitution(NodeReference substitution, TraceNode originalTrace, double rank = double.NaN)
        {
            Substitution = substitution;
            OriginalTrace = originalTrace;

            if (double.IsNaN(rank))
            {
                Rank = OriginalNodes.Count();
            }
        }

        internal PathSubstitution Reranked(double confidence)
        {
            return new PathSubstitution(Substitution, OriginalTrace, confidence);
        }

        internal IEnumerable<NodeReference> FindTargets(ComposedGraph graph)
        {
            var path = OriginalTrace.GetPathToRoot();
            return graph.GetForwardTargets(new[] { Substitution }, path).ToArray();
        }
    }
}