using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.PoolActions
{
    class ExtendAction : IPoolAction
    {
        private readonly KnowledgePath _path;

        public SemanticPart SemanticOrigin { get; private set; }

        public int Priority
        {
            get { return 0; }
        }

        internal ExtendAction(KnowledgePath path)
        {
            _path = path;

            SemanticOrigin = new SemanticPart("", new[] { path });
        }

        public void Run(ContextPool context)
        {
            context.ExtendBy(_path);
        }

        public bool HasSamePoolEffectAs(IPoolAction action)
        {
            throw new NotImplementedException();
        }
    }
}
