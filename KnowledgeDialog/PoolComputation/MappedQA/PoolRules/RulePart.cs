using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class RulePart
    {
        internal readonly RulePart PreviousPart;

        internal readonly RuleBitBase RuleBit;

        internal readonly int PartIndex;

        internal RulePart(RulePart previousPart, RuleBitBase ruleBit)
        {
            if (previousPart != null)
                PartIndex = previousPart.PartIndex + 1;

            PreviousPart = previousPart;
            RuleBit = ruleBit;
        }
    }
}
