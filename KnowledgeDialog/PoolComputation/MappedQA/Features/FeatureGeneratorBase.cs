using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    abstract class FeatureGeneratorBase
    {
        /// <summary>
        /// Template method for generating feautres.
        /// </summary>
        /// <param name="expression">Expression which will be used for features generation.</param>
        /// <param name="graph">Graph grounding generated features.</param>
        /// <returns>Generated features.</returns>
        protected abstract IEnumerable<FeatureInstance> generateFeatures(ParsedUtterance expression, ComposedGraph graph);

        /// <summary>
        /// Generate features.
        /// </summary>
        /// <param name="expression">Expression which will be used for features generation.</param>
        /// <param name="graph">Graph grounding generated features.</param>
        /// <returns>Generated features.</returns>
        public IEnumerable<FeatureInstance> GenerateFeatures(ParsedUtterance expression, ComposedGraph graph)
        {
            return generateFeatures(expression, graph);
        }
    }
}
