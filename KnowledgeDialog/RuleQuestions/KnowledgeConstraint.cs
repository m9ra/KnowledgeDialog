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
        /// Constraint node which defines start of knowledge path.
        /// </summary>
        internal readonly NodeReference Node;

        /// <summary>
        /// The constraint path
        /// </summary>
        internal readonly IEnumerable<Edge> Path;

        internal bool IsSatisfiedBy(NodeReference featureNode, NodeReference answer, ComposedGraph Graph)
        {
            throw new NotImplementedException();
        }
    }
}
