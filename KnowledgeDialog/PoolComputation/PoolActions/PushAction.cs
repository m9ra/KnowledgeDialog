using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.PoolActions
{
    class PushAction : IPoolAction
    {
        public SemanticPart SemanticOrigin { get; private set; }

        public int Priority { get { return 100; } }

        internal PushAction(SemanticPart part)
        {
            SemanticOrigin = part;
        }

        public void Run(ContextPool context)
        {
            var pushStart = context.GetSubstitution(SemanticOrigin.StartNode);
            context.ClearAccumulator();
            foreach (var path in SemanticOrigin.Paths)
            {
                context.Push(pushStart, path);
            }
        }


        public bool HasSamePoolEffectAs(IPoolAction action)
        {
            var o = action as PushAction;
            if (o == null)
                return false;

            var pathCount = SemanticOrigin.Paths.Count();
            var oPathCount = o.SemanticOrigin.Paths.Count();

            if (pathCount != oPathCount)
                return false;

            foreach (var path in SemanticOrigin.Paths)
            {
                if(!o.SemanticOrigin.Paths.Any(p => path.HasSameEdgesAs(p)))
                    return false;
            }

            return true;
        }
    }
}
