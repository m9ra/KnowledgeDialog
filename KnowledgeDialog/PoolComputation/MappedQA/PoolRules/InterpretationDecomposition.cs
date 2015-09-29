using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class RulePart
    {
    }

    class InterpretationDecomposition
    {
        internal readonly IEnumerable<RulePart> Parts;

        internal InterpretationDecomposition(IEnumerable<RulePart> parts){
            Parts = parts.ToArray();
        }
    }
}
