using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class FeatureCover
    {
        public readonly IEnumerable<FeatureInstance> FeatureInstances;

        private readonly bool[] _coveredPositions;

        public FeatureCover(FeatureInstance feature)
        {
            _coveredPositions = new bool[feature.MaxPosition + 1];
            FeatureInstances = new[] { feature };

            indexPositions(feature);
        }

        private FeatureCover(FeatureCover previousCover, FeatureInstance extendingFeature)
        {
            _coveredPositions = previousCover._coveredPositions.ToArray();
            FeatureInstances = previousCover.FeatureInstances.Concat(new[] { extendingFeature }).ToArray();

            indexPositions(extendingFeature);
        }

        internal IEnumerable<FeatureCover> Extend(FeatureInstance feature)
        {
            var hasOverlap = feature.CoveredPositions.Where(p => _coveredPositions[p]).Any();
            if (hasOverlap)
                return new FeatureCover[0];

            return new[]{
                new FeatureCover(this, feature)
            };
        }

        private void indexPositions(FeatureInstance feature)
        {
            //set index of all indexed positions
            foreach (var coveredPosition in feature.CoveredPositions)
            {
                _coveredPositions[coveredPosition] = true;
            }
        }
    }
}
