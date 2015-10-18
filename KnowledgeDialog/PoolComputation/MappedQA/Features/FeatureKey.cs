using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    /// <summary>
    /// Lightweight key of features used for hashing purposes.
    /// </summary>
    class FeatureKey
    {
        private readonly FeatureBase[] _features;

        internal FeatureKey(IEnumerable<FeatureBase> features)
        {
            _features = features.ToArray();
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var acc = 0;
            foreach (var feature in _features)
            {
                acc += feature.GetHashCode();
            }

            return acc;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as FeatureKey;
            if (o == null)
                return false;

            return _features.SequenceEqual(o._features);
        }

        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(" ", (IEnumerable<FeatureBase>)_features));
        }
    }
}
