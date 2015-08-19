using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA
{
    class ContextRule
    {
        internal readonly MappingContext PreContext;

        internal readonly PoolRuleBase Rule;

        internal readonly MappingContext PostContext;
    }
}
