using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    interface IEvaluation
    {
        NodeReference GetSubstitution(NodeReference node);
    }
}
