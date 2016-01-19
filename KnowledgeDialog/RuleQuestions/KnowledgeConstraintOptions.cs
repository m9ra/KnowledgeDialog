using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.RuleQuestions
{
    class KnowledgeConstraintOptions
    {
        internal IEnumerable<KnowledgeConstraint> Constraints;

        internal KnowledgeConstraintOptions(IEnumerable<KnowledgeConstraint> constraints)
        {
            Constraints = constraints.ToArray();
        }
    }
}
