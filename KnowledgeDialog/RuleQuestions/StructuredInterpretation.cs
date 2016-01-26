using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation;
using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.RuleQuestions
{
    class StructuredInterpretation
    {
        /// <summary>
        /// The key for interpretation.
        /// </summary>
        internal readonly FeatureKey FeatureKey;

        /// <summary>
        /// How many of general constraints are there.
        /// </summary>
        internal int GeneralConstraintCount { get { return _generalConstraints.Length; } }

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

        internal StructuredInterpretation(FeatureKey featureKey,IEnumerable<KnowledgeConstraint> generalConstraints, IEnumerable<ConstraintPoolRule> disambiguationConstraints)
        {
            FeatureKey = featureKey;
            _generalConstraints = generalConstraints.ToArray();
            _disambiguationConstraints = disambiguationConstraints.ToArray();
        }

        internal KnowledgeConstraint GetGeneralConstraint(int i)
        {
            return _generalConstraints[i];
        }
    }
}
