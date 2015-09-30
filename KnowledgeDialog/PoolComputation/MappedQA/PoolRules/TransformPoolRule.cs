using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class TransformPoolRule : PoolRuleBase
    {
        private readonly NodeReference _startNode;

        internal TransformPoolRule(NodeReference startNode)
        {
            _startNode = startNode;
        }

        /// <inheritdoc/>
        protected override IEnumerable<RuleBitBase> getRuleBits()
        {
            yield return new RuleHead("Transform");
            yield return new NodeBit(_startNode);
        }
    }
}
