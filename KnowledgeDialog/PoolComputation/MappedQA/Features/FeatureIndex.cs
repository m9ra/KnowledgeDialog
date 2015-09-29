using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class FeatureIndex
    {
        /// <summary>
        /// How many feature positions we have.
        /// </summary>
        public int Length { get { return _features.Length; } }

        /// <summary>
        /// Features indexed by their covering positions.
        /// </summary>
        private readonly List<FeatureInstance>[] _features;

        internal FeatureIndex(IEnumerable<FeatureInstance> features)
        {
            //find range of feature indexes
            var maxIndex = -1;
            foreach (var feature in features)
            {
                foreach (var position in feature.CoveredPositions)
                {
                    if (position > maxIndex)
                        maxIndex = position;
                }
            }

            //index features
            _features = new List<FeatureInstance>[maxIndex + 1];
            for (var i = 0; i < _features.Length; ++i)
                _features[i] = new List<FeatureInstance>();

            foreach (var feature in features)
            {
                foreach (var position in feature.CoveredPositions)
                {
                    _features[position].Add(feature);
                }
            }
        }

        internal IEnumerable<FeatureInstance> GetFeatures(int currentPosition)
        {
            return _features[currentPosition];
        }
    }
}
