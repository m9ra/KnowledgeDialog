using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    abstract class FeatureGeneratorBase
    {
        public IEnumerable<FeatureInstance> GenerateFeatures(ParsedUtterance expression)
        {
            throw new NotImplementedException();
        }
    }
}
