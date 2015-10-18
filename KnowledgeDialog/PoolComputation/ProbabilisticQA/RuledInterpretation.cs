using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class RuledInterpretation
    {
        /// <summary>
        /// Interpretation containing rule.
        /// </summary>
        internal readonly Interpretation Interpretation;

        /// <summary>
        /// Feature which belongs to interpretation
        /// </summary>
        internal readonly FeatureKey FeatureKey;

        public RuledInterpretation(Interpretation interpretation, Interpretation contractedInterpretation, FeatureCover cover, ComposedGraph graph)
        {
            FeatureKey = cover.CreateFeatureKey();

            Interpretation = interpretation.GeneralizeBy(cover, contractedInterpretation, graph);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as RuledInterpretation;
            if (o == null)
                return false;

            return FeatureKey.Equals(o.FeatureKey) && Interpretation.Equals(o.Interpretation);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Interpretation.GetHashCode() + FeatureKey.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}-->{1}", FeatureKey, Interpretation);
        }
    }
}
