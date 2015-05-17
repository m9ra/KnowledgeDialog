using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

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

        internal PushAction Resubstitution(NodesEnumeration originalNodes, NodesEnumeration substitutedNodes)
        {
            NodeReference substitutedNode=null;
            for (var i = 0; i < originalNodes.Count; ++i)
            {
                var originalNode = originalNodes.GetNode(i);
                if (originalNode.Equals(SemanticOrigin.StartNode))
                    substitutedNode = substitutedNodes.GetNode(i);
            }

            var substitutedOrigin = SemanticOrigin;
            if (substitutedNode != null)
                substitutedOrigin = substitutedOrigin.Substitute(SemanticOrigin.StartNode, substitutedNode);

            return new PushAction(substitutedOrigin);
        }
    }
}
