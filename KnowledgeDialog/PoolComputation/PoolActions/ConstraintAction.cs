using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.PoolActions
{
    class ConstraintAction : IPoolAction
    {
        public SemanticPart SemanticOrigin { get; private set; }

        public int Priority { get { return 0; } }

        public readonly KnowledgePath Path;

        public ConstraintAction(SemanticPart constraint, KnowledgePath path)
        {
            this.SemanticOrigin = constraint;
            this.Path = path;
        }

        public void Run(ContextPool context)
        {
            var start = context.GetSubstitution(SemanticOrigin.StartNode);
            var layer = context.GetPathLayer(start, Path.Edges);
            context.RemoveWhere((node) => !layer.Contains(node));
        }

        public bool HasSamePoolEffectAs(IPoolAction action)
        {
            var o = action as ConstraintAction;
            if (o == null)
                return false;

            return Path.HasSameEdgesAs(o.Path);
        }
    }
}
