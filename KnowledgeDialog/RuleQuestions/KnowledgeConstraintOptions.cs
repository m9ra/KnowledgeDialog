using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.RuleQuestions
{
    class KnowledgeConstraintOptions
    {
        internal IEnumerable<KnowledgeConstraint> Constraints { get { return _constraints; } }

        internal int Count { get { return _constraints.Length; } }

        private KnowledgeConstraint[] _constraints;

        internal KnowledgeConstraintOptions(IEnumerable<KnowledgeConstraint> constraints)
        {
            _constraints = constraints.ToArray();
        }

        internal KnowledgeConstraint GetConstraint(int currentIndex)
        {
            return _constraints[currentIndex];
        }
    }
}
