using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    abstract class PoolRuleBase
    {
        /// <summary>
        /// Cache for rule parts.
        /// </summary>
        private IEnumerable<RulePart> _partsCache;

        private IEnumerable<RuleBitBase> _bitsCache;

        protected abstract IEnumerable<RuleBitBase> getRuleBits();

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
    }
}
