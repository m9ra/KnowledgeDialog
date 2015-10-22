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

        internal Interpretation GeneralizeBy(FeatureCover cover, ComposedGraph graph)
        {
            var mapping = initializeNodeMapping(cover, graph);

            var isContractable = _rules.Any(r => r is InsertPoolRule);
            if (mapping.IsEmpty)
            {
                //there is nothing to generalize                
                if (isContractable)
                    //contractable interpretation without generalization is trivial
                    return null;
                else
                    return this;
            }

            mapping.IsGeneralizeMapping = true;
            var newRules = mapRules(mapping);
            if (!mapping.WasUsed && isContractable)
                //mapping wasnt used, so the contractable interpretation is trivial
                return null;

            return new Interpretation(newRules);
        }

        private PoolRuleBase[] mapRules(NodeMapping mapping)
        {
            var newRules = new PoolRuleBase[_rules.Length];
            for (var i = 0; i < _rules.Length; ++i)
            {
                newRules[i] = _rules[i].MapNodes(mapping);
            }
            return newRules;
        }

        internal Interpretation InstantiateBy(FeatureCover cover, ComposedGraph graph)
        {
            var mapping = initializeNodeMapping(cover, graph);

            if (mapping.IsEmpty)
                return this;

            mapping.IsGeneralizeMapping = false;
            var newRules = mapRules(mapping);

            return new Interpretation(newRules);
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
            return string.Format("[Interpretation]{{{0}}}", string.Join(" ", Rules));
        }
    }
}
