using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    /// <summary>
    /// Ordered enumeration of <see cref="KnowledgeDialog.Knowledge.NodeReferences"/>. Is hashable and equitable.
    /// </summary>
    class NodesEnumeration : IEnumerable<NodeReference>
    {
        private readonly NodeReference[] _nodes;

        internal int Count { get { return _nodes.Length; } }

        internal NodesEnumeration(params NodeReference[] nodes)
            : this((IEnumerable<NodeReference>)nodes)
        {
        }

        internal NodesEnumeration(IEnumerable<NodeReference> nodes)
        {
            _nodes = nodes.ToArray();
        }

        internal NodeReference GetNode(int i)
        {
            return _nodes[i];
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var accumulator = 0;
            foreach (var node in _nodes)
            {
                accumulator += node.GetHashCode();
            }
            return accumulator;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as NodesEnumeration;
            if (o == null || o._nodes.Length != _nodes.Length)
                return false;

            for (var i = 0; i < _nodes.Length; ++i)
            {
                if (!o._nodes[i].Equals(_nodes[i]))
                    return false;
            }

            return true;
        }

        public IEnumerator<NodeReference> GetEnumerator()
        {
            return ((IEnumerable<NodeReference>)_nodes).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }
    }
}
