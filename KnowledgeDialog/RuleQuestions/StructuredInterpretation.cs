using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.RuleQuestions
{
    class StructuredInterpretation
    {
        /// <summary>
        /// Constraints are mandatory mapped to input nodes.
        /// </summary>
        internal IEnumerable<KnowledgeConstraint> GeneralConstraints { get { return _generalConstraints; } }

        /// <summary>
        /// Constraints that are generated for disambiguation.
        /// </summary>
        internal IEnumerable<ConstraintPoolRule> DisambiguationConstraints { get { return _disambiguationConstraints; } }

        private readonly KnowledgeConstraint[] _generalConstraints;

        private readonly ConstraintPoolRule[] _disambiguationConstraints;

        internal StructuredInterpretation(IEnumerable<KnowledgeConstraint> generalConstraints, IEnumerable<ConstraintPoolRule> disambiguationConstraints)
        {
            _generalConstraints = generalConstraints.ToArray();
            _disambiguationConstraints = disambiguationConstraints.ToArray();
        }

        internal KnowledgeConstraint GetGeneralConstraint(int i)
        {
            return _generalConstraints[i];
        }
    }
}
