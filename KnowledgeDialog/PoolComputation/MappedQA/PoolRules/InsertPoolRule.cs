using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation.MappedQA.Features;

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

        /// <inheritdoc/>
        protected override void execute(ContextPool pool)
        {
            pool.ClearAccumulator();
            pool.Insert(_insertedNode);
        }

        /// <inheritdoc/>
        protected override PoolRuleBase mapNodes(NodeMapping mapping)
        {
            return new InsertPoolRule(mapping.GetMappedNode(_insertedNode));
        }

        /// <inheritdoc/>
        protected override IEnumerable<PoolRuleBase> extend(NodeReference node, ComposedGraph graph)
        {
            yield break;
        }
    }
}
