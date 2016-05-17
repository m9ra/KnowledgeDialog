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

        public readonly NodeReference StartNode;

        internal SemanticPart(string utterance, IEnumerable<KnowledgePath> paths)
        {
            _paths = paths.ToArray();

            if (_paths.Length < 0)
                throw new NotSupportedException("Cannot create empty SemanticPart");

            StartNode = _paths[0].Node(0);
            Utterance = utterance;
        }

        private SemanticPart(string utterance, NodeReference startNode, IEnumerable<KnowledgePath> paths)
        {
            Utterance = utterance;
            StartNode = startNode;
            _paths = paths.ToArray();
        }

        internal SemanticPart Substitute(NodeReference originalReference, NodeReference substitutedNode)
        {
            //TODO for now it is sufficient to have only StartNode substitution
            //but this is only because of using only path edges !!!!
            var startNode = StartNode;
            if (StartNode.Equals(originalReference))
                startNode = substitutedNode;

            return new SemanticPart(Utterance, startNode, Paths);
        }
    }
}
