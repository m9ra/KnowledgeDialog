using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    abstract class FeatureGeneratorBase
    {
        /// <summary>
        /// Template method for generating feautres.
        /// </summary>
        /// <param name="expression">Expression which will be used for features generation.</param>
        /// <returns>Generated features.</returns>
        protected abstract IEnumerable<FeatureInstance> generateFeatures(ParsedUtterance expression);

        /// <summary>
        /// Generate features.
        /// </summary>
        /// <param name="expression">Expression which will be used for features generation.</param>
        /// <returns>Generated features.</returns>
        public IEnumerable<FeatureInstance> GenerateFeatures(ParsedUtterance expression)
        {
            return generateFeatures(expression);
        }
    }
}
