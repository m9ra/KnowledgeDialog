using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PatternComputation.Actions
{
    public abstract class ActionBase
    {
        public abstract IEnumerable<NodeReference> ContextNodes { get; }

        public abstract ResponseBase Execute(EvaluationContext evaluationContext, KnowledgeGroup contextGroup);
    }
}
