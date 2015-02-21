using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class SemanticPart
    {
        private readonly KnowledgePath[] _paths;

        public readonly string Utterance;

        public IEnumerable<KnowledgePath> Paths { get { return _paths; } }

        public NodeReference StartNode { get { return _paths[0].Node(0); } }

        internal SemanticPart(string utterance, IEnumerable<KnowledgePath> paths)
        {
            Utterance = utterance;
            _paths = paths.ToArray();

            if (_paths.Length < 0)
                throw new NotSupportedException("Cannot create empty SemanticPart");
        }
    }
}
