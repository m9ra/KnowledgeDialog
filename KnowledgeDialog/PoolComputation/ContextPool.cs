using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class ContextPool
    {
        public int ActiveCount { get { return _accumulator.Count; } }

        public bool HasActive { get { return ActiveCount > 0; } }

        public IEnumerable<NodeReference> ActiveNodes { get { return _accumulator; } }

        private Dictionary<NodeReference, NodeReference> _substitutions = new Dictionary<NodeReference, NodeReference>();

        private HashSet<NodeReference> _accumulator = new HashSet<NodeReference>();

        public readonly ComposedGraph Graph;

        public ContextPool(ComposedGraph context)
        {
            Graph = context;
        }


        internal ContextPool Clone()
        {
            var clone = new ContextPool(Graph);
            clone._accumulator.UnionWith(_accumulator);
            clone._substitutions = new Dictionary<NodeReference, NodeReference>(_substitutions);

            return clone;
        }

        internal NodeReference GetSubstitution(NodeReference node)
        {
            NodeReference substitution;
            if (!_substitutions.TryGetValue(node, out substitution))
                return node;

            return substitution;
        }

        internal void SetSubstitutions(IEnumerable<KeyValuePair<NodeReference, NodeReference>> substitutions)
        {
            _substitutions = new Dictionary<NodeReference, NodeReference>();
            foreach (var substitution in substitutions)
            {
                _substitutions[substitution.Key] = substitution.Value;
            }
        }

        internal void ClearAccumulator()
        {
            _accumulator.Clear();
        }

        internal void Push(NodeReference pushStart, KnowledgePath path)
        {
            var layer = GetPathLayer(pushStart, path);

            _accumulator = layer;
        }

        internal void ExtendBy(KnowledgePath path)
        {
            var extended = Graph.GetForwardTargets(ActiveNodes, path);
            _accumulator.Clear();
            _accumulator.UnionWith(extended);
        }

        internal void Insert(params NodeReference[] nodes)
        {
            _accumulator.UnionWith(nodes);
        }

        internal HashSet<NodeReference> GetPathLayer(NodeReference pushStart, KnowledgePath path)
        {
            var layer = new HashSet<NodeReference>();
            var newLayer = new HashSet<NodeReference>();

            layer.Add(pushStart);
            for (var i = 0; i < path.Length; ++i)
            {
                var edge = path.Edge(i);
                var isOutcomming = path.IsOutcomming(i);
                foreach (var node in layer)
                {
                    var nextNodes = Graph.Targets(node, edge, isOutcomming);
                    newLayer.UnionWith(nextNodes);
                }

                layer = newLayer;
                newLayer = new HashSet<NodeReference>();
            }
            return layer;
        }

        internal void RemoveWhere(Predicate<NodeReference> condition)
        {
            var copy = _accumulator.ToArray();
            foreach (var node in copy)
            {
                if (condition(node))
                    _accumulator.Remove(node);
            }
        }
    }
}
