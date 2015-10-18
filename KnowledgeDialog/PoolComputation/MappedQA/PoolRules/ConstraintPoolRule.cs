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

        private readonly Tuple<string, bool>[] _edges;

        private readonly NodeReference _targetNode;

        internal ConstraintPoolRule(PathSegment constraintPath)
        {
            _targetNode = constraintPath.Node;
            _edges = constraintPath.GetEdges().ToArray();
        }

        private ConstraintPoolRule(NodeReference targetNode, Tuple<string, bool>[] edges)
        {
            _targetNode = targetNode;
            _edges = edges;
        }

        protected override IEnumerable<RuleBitBase> getRuleBits()
        {
            yield return new RuleHead("Constraint");
            for (var i = 0; i < _edges.Length; ++i)
            {
                var edge = _edges[i];
                yield return new EdgeBit(edge.Item1, edge.Item2);
            }
        }

        /// <inheritdoc/>
        protected override void execute(ContextPool pool)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override PoolRuleBase mapNodes(NodeMapping mapping)
        {
            return new ConstraintPoolRule(mapping.GetMappedNode(_targetNode), _edges);
        }
    }
}
