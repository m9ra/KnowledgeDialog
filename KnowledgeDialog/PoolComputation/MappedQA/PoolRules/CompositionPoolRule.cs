using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class CompositionPoolRule : PoolRuleBase
    {
        private readonly PoolRuleBase[] _composedRules;

        internal CompositionPoolRule(IEnumerable<PoolRuleBase> composedRules)
        {
            _composedRules = composedRules.ToArray();
        }


        /// <inheritdoc/>
        protected override IEnumerable<RuleBitBase> getRuleBits()
        {
            return _composedRules.SelectMany(r => r.RuleBits);
        }

        /// <inheritdoc/>
        protected override void execute(ContextPool pool)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override PoolRuleBase mapNodes(NodeMapping mapping)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override IEnumerable<PoolRuleBase> extend(NodeReference node, ComposedGraph graph)
        {
            yield break;
        }
    }
}
