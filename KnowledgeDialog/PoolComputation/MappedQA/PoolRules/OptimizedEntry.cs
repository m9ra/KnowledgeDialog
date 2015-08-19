using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;


namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class FeaturedRule
    {
        internal readonly FeatureBase Feature;

        internal readonly ContextRule Rule;
    }

    class OptimizedEntry
    {
        private readonly FeatureCover[] _covers;

        private readonly InterpretationsFactory _interpretations;

        public IEnumerable<FeaturedRule> CurrentFeaturedRules;

        internal OptimizedEntry(InterpretationsFactory interpretations, IEnumerable<FeatureCover> covers)
        {
            _covers = covers.ToArray();
            _interpretations = interpretations;
        }
    }
}
