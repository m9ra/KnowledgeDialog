using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class Interpretation
    {
        private readonly PoolRuleBase[] _rules;

        internal IEnumerable<PoolRuleBase> Rules { get { return _rules; } }

        internal Interpretation(IEnumerable<PoolRuleBase> rules)
        {
            _rules = rules.ToArray();
        }

        internal Interpretation GeneralizeBy(FeatureCover cover)
        {
            var mapping = initializeNodeMapping(cover);

            if (mapping.IsEmpty)
                return this;

            throw new NotImplementedException();
        }

        internal Interpretation InstantiateBy(FeatureCover cover)
        {
            var mapping = initializeNodeMapping(cover);

            if (mapping.IsEmpty)
                return this;

            throw new NotImplementedException();
        }

        private NodeMapping initializeNodeMapping(FeatureCover cover)
        {
            var mapping = new NodeMapping();
            foreach (var instance in cover.FeatureInstances)
            {
                instance.SetMapping(mapping);
            }

            return mapping;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/> 
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
