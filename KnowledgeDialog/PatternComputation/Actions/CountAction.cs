using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.PatternComputation.Actions
{
    class CountAction : ActionBase
    {
        public readonly NodeReference CountedNode;

        public readonly Tuple<string, bool> Edge;

        public override IEnumerable<NodeReference> ContextNodes
        {
            get { yield return CountedNode; }
        }

        public CountAction(NodeReference countedNode, Tuple<string, bool> edge)
        {
            CountedNode = countedNode;
            Edge = edge;
        }

        public override ResponseBase Execute(EvaluationContext evaluationContext, KnowledgeGroup contextGroup)
        {
            var responseNode = evaluationContext.GetSubstitution(CountedNode, contextGroup);
            var targets = evaluationContext.Graph.Targets(responseNode, Edge.Item1, Edge.Item2);
            var count = targets.Count();

            return new CountResponse(count);
        }
    }
}
