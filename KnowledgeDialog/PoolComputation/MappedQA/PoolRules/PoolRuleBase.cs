using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    abstract class PoolRuleBase
    {
        /// <summary>
        /// Cache for rule parts.
        /// </summary>
        private IEnumerable<RulePart> _partsCache;

        private IEnumerable<RuleBitBase> _bitsCache;

        private string _ruleRepresentationCache;

        protected abstract IEnumerable<RuleBitBase> getRuleBits();

        protected abstract void execute(ContextPool pool);

        protected abstract PoolRuleBase mapNodes(NodeMapping mapping);

        internal IEnumerable<RulePart> Parts
        {
            get
            {
                if (_partsCache == null)
                    //cache parts
                    _partsCache = createRuleParts(RuleBits);

                return _partsCache;
            }
        }

        internal IEnumerable<RuleBitBase> RuleBits
        {
            get
            {
                if (_bitsCache == null)
                    _bitsCache = getRuleBits();

                return _bitsCache;
            }
        }

        internal string RuleRepresentation
        {
            get
            {
                if (_ruleRepresentationCache == null)
                    _ruleRepresentationCache = string.Join(" ", RuleBits);

                return _ruleRepresentationCache;
            }
        }

        internal void Execute(ContextPool pool)
        {
            execute(pool);
        }

        internal PoolRuleBase MapNodes(NodeMapping mapping)
        {
            var mappedRule = mapNodes(mapping);
            return mappedRule;
        }

        private IEnumerable<RulePart> createRuleParts(IEnumerable<RuleBitBase> ruleBits)
        {
            var result = new List<RulePart>();

            RulePart previousPart = null;
            foreach (var ruleBit in ruleBits)
            {
                var currentPart = new RulePart(previousPart, ruleBit);
                previousPart = currentPart;

                result.Add(currentPart);
            }

            return result;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as PoolRuleBase;
            if (o == null)
                return false;

            return RuleRepresentation == o.RuleRepresentation;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return RuleRepresentation.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return RuleRepresentation;
        }
    }
}
