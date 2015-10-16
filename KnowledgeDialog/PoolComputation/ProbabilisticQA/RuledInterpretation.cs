using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class RuledInterpretation
    {
        /// <summary>
        /// Interpretation containing rule.
        /// </summary>
        private readonly Interpretation _interpretation;

        /// <summary>
        /// Feature which belongs to interpretation
        /// </summary>
        internal readonly FeatureKey FeatureKey;

        public RuledInterpretation(Interpretation interpretation, FeatureCover cover)
        {
            FeatureKey = cover.CreateFeatureKey();

            _interpretation = interpretation.GeneralizeBy(cover);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as RuledInterpretation;
            if (o == null)
                return false;

            return FeatureKey.Equals(o.FeatureKey) && _interpretation.Equals(o._interpretation);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _interpretation.GetHashCode() + FeatureKey.GetHashCode();
        }
    }
}
