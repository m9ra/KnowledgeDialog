using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class TransformPoolRule : PoolRuleBase
    {
        private readonly bool[] _isOutgoing;

        private readonly string[] _edges;

        internal TransformPoolRule(PathSegment startingSegment)
        {
            var isOutgoing = new List<bool>();
            var edges = new List<string>();

            var currentSegment = startingSegment;
            while (currentSegment != null)
            {
                //collect path segments
                isOutgoing.Add(currentSegment.IsOutcoming);
                edges.Add(currentSegment.Edge);

                currentSegment = currentSegment.PreviousSegment;
            }

            _isOutgoing = isOutgoing.ToArray();
            _edges = edges.ToArray();
        }

        protected override IEnumerable<RuleBitBase> getRuleBits()
        {
            throw new NotImplementedException();
        }
    }
}
