using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.RuleQuestions
{
    class KnowledgeConstraint
    {
        /// <summary>
        /// The constraint path
        /// </summary>
        internal readonly IEnumerable<Edge> Path;

        internal KnowledgeConstraint(KnowledgePath path)
        {
            Path = path.Edges;
        }

        internal bool IsSatisfiedBy(NodeReference featureNode, NodeReference answer, ComposedGraph graph)
        {
            return FindSet(featureNode, graph).Contains(answer);
        }

        internal HashSet<NodeReference> FindSet(NodeReference constraintNode,ComposedGraph graph)
        {
            return new HashSet<NodeReference>(graph.GetForwardTargets(new[] { constraintNode }, Path));
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as KnowledgeConstraint;
            if (o == null)
                return false;

            return Enumerable.SequenceEqual(Path, o.Path);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var acc = 0;
            foreach (var edge in Path)
            {
                acc += edge.GetHashCode();
            }

            return acc;
        }
    }
}
