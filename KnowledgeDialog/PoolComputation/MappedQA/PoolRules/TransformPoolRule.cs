using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class TransformPoolRule : PoolRuleBase
    {
        private readonly Edge[] _edges;

        internal TransformPoolRule(PathSegment startingSegment)
        {
            _edges = startingSegment.GetInvertedEdges().ToArray();
        }

        /// <inheritdoc/>
        protected override IEnumerable<RuleBitBase> getRuleBits()
        {
            yield return new RuleHead("Transform");
            for (var i = 0; i < _edges.Length; ++i)
            {
                var edge = _edges[i];
                yield return new EdgeBit(edge);
            }
        }

        /// <inheritdoc/>
        protected override void execute(ContextPool pool)
        {
            pool.ExtendBy(_edges);
        }

        /// <inheritdoc/>
        protected override PoolRuleBase mapNodes(NodeMapping mapping)
        {
            //no nodes can be mapped in Transform rule
            return this;
        }

        /// <inheritdoc/>
        protected override IEnumerable<PoolRuleBase> extend(NodeReference node, ComposedGraph graph)
        {
            yield break;
        }
    }
}
