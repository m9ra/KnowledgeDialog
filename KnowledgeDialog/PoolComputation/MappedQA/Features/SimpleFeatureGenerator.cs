using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class SimpleFeatureGenerator : FeatureGeneratorBase
    {
        protected override IEnumerable<FeatureInstance> generateFeatures(ParsedUtterance expression, ComposedGraph graph)
        {
            return new[] { CreateSimpleFeatureInstance(expression) };
        }

        internal static FeatureInstance CreateSimpleFeatureInstance(ParsedUtterance origin)
        {
            var coveredPositions = new int[origin.Words.Count()];
            for (var i = 0; i < coveredPositions.Length; ++i)
                coveredPositions[i] = i;

            return new FeatureInstance(origin, new SimpleFeature(origin), coveredPositions);
        }
    }
}
