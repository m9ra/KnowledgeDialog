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

        internal void SetSubstitutions(NodesSubstitution substitutions)
        {
            _substitutions = new Dictionary<NodeReference, NodeReference>();

            if (substitutions == null)
                //nothing more to substitute
                return;

            for (var nodeIndex = 0; nodeIndex < substitutions.NodeCount; ++nodeIndex)
            {
                var key = substitutions.GetOriginalNode(nodeIndex);
                var substitution = substitutions.GetSubstitution(nodeIndex);
                _substitutions[key] = substitution;
            }
        }

        internal void ClearAccumulator()
        {
            _accumulator.Clear();
        }

        internal void Push(NodeReference pushStart, KnowledgePath path)
        {
            var layer = GetPathLayer(pushStart, path.Edges);
            _accumulator.UnionWith(layer);
        }

        internal void ExtendBy(IEnumerable<Edge> edges)
        {
            var extended = Graph.GetForwardTargets(ActiveNodes, edges);
            _accumulator.Clear();
            _accumulator.UnionWith(extended);
        }

        internal void Insert(params NodeReference[] nodes)
        {
            _accumulator.UnionWith(nodes);
        }

        internal HashSet<NodeReference> GetPathLayer(NodeReference pushStart, IEnumerable<Edge> edges)
        {
            var layer = new HashSet<NodeReference>();
            var newLayer = new HashSet<NodeReference>();

            layer.Add(pushStart);
            foreach(var edge in edges)
            {
                foreach (var node in layer)
                {
                    var nextNodes = Graph.Targets(node, edge);
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

        internal void Filter(KnowledgeClassifier<bool> knowledgeClassifier)
        {
            if (knowledgeClassifier.Root == null)
                //there is no initialization yet
                return;

            var newLayer = new HashSet<NodeReference>();
            foreach (var node in _accumulator)
            {
                var isAccepted = knowledgeClassifier.Classify(node);
                if (isAccepted)
                    newLayer.Add(node);
            }

            _accumulator = newLayer;
        }

        internal bool ContainsInAccumulator(NodeReference nodeReference)
        {
            return _accumulator.Contains(nodeReference);
        }
    }
}
