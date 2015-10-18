using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class NodeFeature : FeatureBase
    {
        /// <summary>
        /// Index of the feature.
        /// </summary>
        internal readonly int Index;

        internal NodeFeature(int index)
        {
            Index = index;
        }

        /// <inheritdoc/>
        protected override int getHashCode()
        {
            return Index.GetHashCode();
        }

        /// <inheritdoc/>
        protected override bool equals(FeatureBase featureBase)
        {
            var o = featureBase as NodeFeature;
            if (o == null)
                return false;

            return Index == o.Index;
        }

        /// <inheritdoc/>
        protected override string toString()
        {
            return "$" + Index;
        }

        /// <inheritdoc/>
        protected override double probability(RulePart part)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void setMapping(FeatureInstance featureInstance, NodeMapping mapping)
        {
            var instanceNodeData = featureInstance.Origin.Words.Skip(Index).First();
            var generalNodeData = "$" + Index;
            mapping.SetMapping(instanceNodeData, generalNodeData);
        }
    }
}
