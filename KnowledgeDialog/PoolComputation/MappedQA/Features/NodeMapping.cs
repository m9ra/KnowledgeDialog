using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    class NodeMapping
    {
        internal bool IsEmpty { get { return _mapping.Count == 0; } }

        private readonly Dictionary<NodeReference, NodeReference> _mapping = new Dictionary<NodeReference, NodeReference>();

    }
}
