using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.RuleQuestions
{
    class StructuredInterpretation
    {
        /// <summary>
        /// Constraints are mandatory mapped to input nodes.
        /// </summary>
        internal readonly IEnumerable<KnowledgeConstraint> MappedConstraints;

        /// <summary>
        /// Constraints that are generated for disambiguation.
        /// </summary>
        internal readonly IEnumerable<KnowledgeConstraint> DisambiguationConstraints;
    }
}
