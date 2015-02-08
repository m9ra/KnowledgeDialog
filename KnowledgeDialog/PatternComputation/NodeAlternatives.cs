using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    class NodeAlternatives
    {
        private readonly NodeReference _node;

        private readonly NodeReference[] _alternatives;

        private int _currentIndex = 0;

        public NodeAlternatives(NodeReference node, IEnumerable<NodeReference> alternatives)
        {
            _node = node;
            _alternatives = alternatives.ToArray();
        }
        internal NodeReference Next()
        {
            if (_currentIndex >= _alternatives.Length)
                return null;
            return _alternatives[_currentIndex++];
        }
    }
}
