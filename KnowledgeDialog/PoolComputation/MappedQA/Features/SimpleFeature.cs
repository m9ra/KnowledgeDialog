using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class SimpleFeature : FeatureBase
    {
        private readonly ParsedUtterance _utterance;

        internal SimpleFeature(ParsedUtterance utterance)
        {
            _utterance = utterance;
        }

        /// <inheritdoc/>
        protected override int getHashCode()
        {
            return _utterance.GetHashCode();
        }

        /// <inheritdoc/>
        protected override bool equals(FeatureBase featureBase)
        {
            var uF = featureBase as SimpleFeature;
            if (uF == null)
                return false;

            return _utterance.Equals(uF._utterance);
        }

        /// <inheritdoc/>
        protected override double probability(RulePart part)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void setMapping(FeatureInstance featureInstance, NodeMapping mapping)
        {
            //nothing to do
        }

    }
}
