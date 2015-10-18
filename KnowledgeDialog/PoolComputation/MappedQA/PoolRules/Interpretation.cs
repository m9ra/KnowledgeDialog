using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class Interpretation
    {
        private readonly PoolRuleBase[] _rules;

        internal IEnumerable<PoolRuleBase> Rules { get { return _rules; } }

        internal Interpretation(IEnumerable<PoolRuleBase> rules)
        {
            //TODO check ordering
            _rules = rules.ToArray();
        }

        internal Interpretation GeneralizeBy(FeatureCover cover, Interpretation contractedInterpretation, ComposedGraph graph)
        {
            var mapping = initializeNodeMapping(cover, graph);

            if (mapping.IsEmpty)
            {
                //there is nothing to generalize
                var isContractable = _rules.Any(r => r is InsertPoolRule);
                if (isContractable)
                    //contractable interpretation can be simplified
                    return contractedInterpretation;
                else
                    return this;
            }

            mapping.IsGeneralizeMapping = true;
            var newRules = new PoolRuleBase[_rules.Length];
            for (var i = 0; i < _rules.Length; ++i)
            {
                newRules[i] = _rules[i].MapNodes(mapping);
            }

            return new Interpretation(newRules);
        }

        internal Interpretation InstantiateBy(FeatureCover cover, ComposedGraph graph)
        {
            var mapping = initializeNodeMapping(cover, graph);

            if (mapping.IsEmpty)
                return this;

            mapping.IsGeneralizeMapping = false;

            throw new NotImplementedException();
        }

        private NodeMapping initializeNodeMapping(FeatureCover cover, ComposedGraph graph)
        {
            var mapping = new NodeMapping(graph);
            foreach (var instance in cover.FeatureInstances)
            {
                instance.SetMapping(mapping);
            }

            return mapping;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as Interpretation;
            if (o == null)
                return false;

            return _rules.SequenceEqual(o._rules);
        }

        /// <inheritdoc/> 
        public override int GetHashCode()
        {
            var hashcode = 0;
            foreach (var rule in _rules)
            {
                hashcode += rule.GetHashCode();
            }

            return hashcode;
        }

        public override string ToString()
        {
            return string.Format("[Interpretation]{{{0}}}", string.Join(" and ", Rules));
        }
    }
}
