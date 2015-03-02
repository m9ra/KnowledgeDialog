using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation.Actions
{
    class InformAction : ActionBase
    {
        public readonly NodeReference InformNode;

        public override IEnumerable<NodeReference> ContextNodes
        {
            get { yield return InformNode; }
        }

        public InformAction(SimpleResponse response, ComposedGraph contextGraph)
        {
            InformNode = contextGraph.GetNode(response.ResponseText);
        }

        public override ResponseBase Execute(EvaluationContext evaluationContext, KnowledgeGroup contextGroup)
        {
            var responseNode = evaluationContext.GetSubstitution(InformNode, contextGroup);
            if (responseNode == null)
                responseNode = InformNode;

            return new SimpleResponse(responseNode.Data.ToString());
        }
    }
}
