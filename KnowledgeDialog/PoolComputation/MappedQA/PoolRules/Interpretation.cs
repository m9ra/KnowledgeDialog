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
            : this(rules.ToArray())
        {
        }

        private Interpretation(PoolRuleBase[] rules)
        {
            _rules = rules;
        }

        internal Interpretation GeneralizeBy(FeatureCover cover, ComposedGraph graph)
        {
            var mapping = cover.CreateNodeMapping(graph);

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

        internal Interpretation InstantiateBy(FeatureCover cover, ComposedGraph graph)
        {
            var mapping = cover.CreateNodeMapping(graph);

            if (mapping.IsEmpty)
                return this;

            mapping.IsGeneralizeMapping = false;
            var newRules = mapRules(mapping);

            return new Interpretation(newRules);
        }

        internal IEnumerable<Interpretation> ExtendBy(IEnumerable<NodeReference> enumerable, ComposedGraph graph)
        {
            foreach (var node in enumerable)
            {
                for (var i = 0; i < _rules.Length; ++i)
                {
                    var extendedRules = _rules[i].Extend(node, graph);
                    foreach (var extendedRule in extendedRules)
                    {
                        var ruleCopy = (PoolRuleBase[])_rules.Clone();
                        ruleCopy[i] = extendedRule;

                        yield return new Interpretation(ruleCopy);
                    }
                }
            }
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
