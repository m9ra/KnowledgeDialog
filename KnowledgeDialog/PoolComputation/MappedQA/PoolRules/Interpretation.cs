using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class Interpretation
    {
        private readonly PoolRuleBase[] _rules;

        internal IEnumerable<PoolRuleBase> Rules { get { return _rules; } }

        internal Interpretation(IEnumerable<PoolRuleBase> rules)
        {
            _rules = rules.ToArray();
        }
    }
}
