using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class InsertPoolRule : PoolRuleBase
    {
        private readonly NodeReference _insertedNode;

        internal InsertPoolRule(NodeReference insertedNode)
        {
            _insertedNode = insertedNode;
        }

        /// <inheritdoc/>
        protected override IEnumerable<RuleBitBase> getRuleBits()
        {
            yield return new RuleHead("Insert");
            yield return new NodeBit(_insertedNode);
        }
    }
}
