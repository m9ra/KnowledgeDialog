using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation
{
    interface IPoolAction
    {
        SemanticPart SemanticOrigin { get; }

        int Priority { get; }

        void Run(ContextPool context);

        bool HasSamePoolEffectAs(IPoolAction action);
    }
}
