using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class ReferencedPartCluster : PartCluster
    {
        internal ReferencedPartCluster(PartCluster cluster)
            : base(cluster.Feature, cluster.Parts)
        {
            throw new NotImplementedException();
        }
    }
}
