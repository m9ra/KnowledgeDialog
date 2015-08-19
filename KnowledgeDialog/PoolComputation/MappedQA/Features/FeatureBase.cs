using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class FeatureBase
    {
    }

    class FeatureInstance
    {
        internal readonly ParsedUtterance Origin;

        internal readonly FeatureBase Feature;

        internal readonly IEnumerable<int> CoveredPositions;
    }
}
