using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class UnigramFeature : FeatureBase
    {
        internal readonly string Word;

        internal UnigramFeature(string word)
        {
            Word = word;
        }

        /// <inheritdoc/>
        protected override int getHashCode()
        {
            return Word.GetHashCode();
        }

        /// <inheritdoc/>
        protected override bool equals(FeatureBase featureBase)
        {
            var uF = featureBase as UnigramFeature;
            if (uF == null)
                return false;

            return Word.Equals(uF.Word);
        }

        /// <inheritdoc/>
        protected override double probability(RulePart part)
        {
            var nodeBit = part.RuleBit as NodeBit;
            if (nodeBit == null)
                return 0;

            var nodeData=nodeBit.Node.Data.ToString();
            if (nodeData == Word)
                return 1.0;

            return 0.0;
        }

        /// <inheritdoc/>
        protected override void setMapping(FeatureInstance featureInstance, NodeMapping mapping)
        {
            //nothing to do
        }
    }
}
