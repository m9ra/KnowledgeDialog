using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class FeatureCover
    {
        public readonly IEnumerable<FeatureInstance> FeatureInstances;

        private readonly bool[] _coveredPositions;

        private static readonly List<FeatureGeneratorBase> _featureGenerators = new List<FeatureGeneratorBase>(){
            new SimpleFeatureGenerator(),
            new UnigramFeatureGenerator()
        };

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


        #region Feature cover creation

        internal static IEnumerable<FeatureCover> GetFeatureCovers(ParsedUtterance expression)
        {
            var features = createFeatures(expression);
            var index = new FeatureIndex(features);

            return generateCovers(index, 0);
        }

        private static IEnumerable<FeatureInstance> createFeatures(ParsedUtterance expression)
        {
            var features = new HashSet<FeatureInstance>();
            foreach (var generator in _featureGenerators)
            {
                features.UnionWith(generator.GenerateFeatures(expression));
            }

            return features;
        }

        private static IEnumerable<FeatureCover> generateCovers(FeatureIndex index, int currentPosition)
        {
            if (index.Length == 0)
                //there are no possible covers
                return new FeatureCover[0];

            if (index.Length - 1 == currentPosition)
            {
                //we are at bottom of recursion
                //we will initiate covers that will be extended later through the recursion

                var newCovers = new List<FeatureCover>();
                foreach (var feature in index.GetFeatures(currentPosition))
                {
                    var cover = new FeatureCover(feature);
                    newCovers.Add(cover);
                }

                return newCovers;
            }

            //extend covers that we got from deeper recursion
            var previousCovers = generateCovers(index, currentPosition + 1);
            var extendedCovers = new List<FeatureCover>();
            foreach (var cover in previousCovers)
            {
                foreach (var feature in index.GetFeatures(currentPosition))
                {
                    extendedCovers.AddRange(cover.Extend(feature));
                }
            }

            return extendedCovers;
        }
        #endregion
    }
}
