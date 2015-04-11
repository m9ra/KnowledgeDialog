using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.PoolActions
{
    class InsertAction: IPoolAction
    {
        public SemanticPart SemanticOrigin { get; private set; }

        public int Priority { get { return 100; } }

        public readonly NodeReference Node;

        public InsertAction(NodeReference node)
        {
            Node = node;
        }


        public void Run(ContextPool context)
        {
            context.ClearAccumulator();
            context.Insert(Node);
        }
            }
}
