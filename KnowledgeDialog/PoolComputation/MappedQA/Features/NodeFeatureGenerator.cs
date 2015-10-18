using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class NodeFeatureGenerator : FeatureGeneratorBase
    {

        /// <inheritdoc/>
        protected override IEnumerable<FeatureInstance> generateFeatures(ParsedUtterance expression, ComposedGraph graph)
        {
            var featureInstances = new List<FeatureInstance>();
            var wordIndex = 0;
            foreach (var word in expression.Words)
            {
                if (graph.HasEvidence(word))
                {
                    var nodeFeature = new NodeFeature(wordIndex);
                    var instance = new FeatureInstance(expression, nodeFeature, wordIndex);
                    featureInstances.Add(instance);
                }
                ++wordIndex;
            }

            return featureInstances;
        }
    }
}
