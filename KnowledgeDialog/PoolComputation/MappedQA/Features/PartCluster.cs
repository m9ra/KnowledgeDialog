using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA
{
    /// <summary>
    /// Wraps parts, ordered in interpretation order, mapped to a feature .
    /// </summary>
    class PartCluster
    {
        internal readonly FeatureBase Feature;

        internal readonly IEnumerable<RulePart> Parts;

        public PartCluster(FeatureBase feature, IEnumerable<RulePart> parts)
        {
            Feature = feature;

            Parts=parts.OrderBy(part => part.PartIndex).ToArray();
        }

    }
}
