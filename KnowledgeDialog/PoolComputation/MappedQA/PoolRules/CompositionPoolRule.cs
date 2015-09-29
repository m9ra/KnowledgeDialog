using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class CompositionPoolRule : PoolRuleBase
    {
        private readonly PoolRuleBase[] _composedRules;

        internal CompositionPoolRule(IEnumerable<PoolRuleBase> composedRules)
        {
            _composedRules = composedRules.ToArray();
        }
    }
}
