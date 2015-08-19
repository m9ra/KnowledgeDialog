using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class FeatureCover
    {
        public IEnumerable<FeatureBase> Features { get { throw new NotImplementedException(); } }

        private FeatureInstance _feature;

        public FeatureCover(FeatureInstance feature)
        {
            _feature = feature;
        }

        internal IEnumerable<FeatureCover> Extend(FeatureInstance feature)
        {
            throw new NotImplementedException();
        }
    }
}
