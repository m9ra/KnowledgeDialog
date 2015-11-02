using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class ConstraintPoolRule : PoolRuleBase
    {
        internal int ConstraintLength { get { return _edges.Length; } }

        internal IEnumerable<Tuple<string, bool>> Edges { get { return _edges; } }

        private readonly Tuple<string, bool>[] _edges;

        private readonly NodeReference _targetNode;

        internal ConstraintPoolRule(PathSegment constraintPath)
        {
            _targetNode = constraintPath.Node;
            _edges = constraintPath.GetInvertedEdges().ToArray();
        }

        private ConstraintPoolRule(NodeReference targetNode, Tuple<string, bool>[] edges)
        {
            _targetNode = targetNode;
            _edges = edges;
        }

        protected override IEnumerable<RuleBitBase> getRuleBits()
        {
            yield return new RuleHead("Constraint");
            yield return new NodeBit(_targetNode);
            for (var i = 0; i < _edges.Length; ++i)
            {
                var edge = _edges[i];
                yield return new EdgeBit(edge.Item1, edge.Item2);
            }
        }

        /// <inheritdoc/>
        protected override void execute(ContextPool pool)
        {
            var layer = pool.GetPathLayer(_targetNode, _edges);
            pool.RemoveWhere((n) => !layer.Contains(n));
        }

        /// <inheritdoc/>
        protected override PoolRuleBase mapNodes(NodeMapping mapping)
        {
            return new ConstraintPoolRule(mapping.GetMappedNode(_targetNode), _edges);
        }

        /// <inheritdoc/>
        protected override IEnumerable<PoolRuleBase> extend(NodeReference node, ComposedGraph graph)
        {
            foreach (var path in graph.GetPaths(_targetNode, node, 3, 100))
            {
                yield return extendWith(path);
            }

            yield break;
        }

        private PoolRuleBase extendWith(KnowledgePath path)
        {
            var extendingEdges = path.CompleteInversedEdges;
            var edges = extendingEdges.Concat(_edges).ToArray();
            var extendingNode = path.Node(path.Length);
            return new ConstraintPoolRule(extendingNode, edges);
        }
    }
}
