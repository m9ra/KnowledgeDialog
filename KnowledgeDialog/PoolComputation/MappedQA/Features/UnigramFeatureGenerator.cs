using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class UnigramFeatureGenerator : FeatureGeneratorBase
    {
        protected override IEnumerable<FeatureInstance> generateFeatures(ParsedUtterance expression, ComposedGraph graph)
        {
            var featureInstances = new List<FeatureInstance>();
            var wordIndex = 0;
            foreach (var word in expression.Words)
            {
                var feature = new UnigramFeature(word);
                var featureInstance = new FeatureInstance(expression, feature, wordIndex);
                featureInstances.Add(featureInstance);
                ++wordIndex;
            }

            return featureInstances;
        }
    }
}
