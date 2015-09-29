using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
