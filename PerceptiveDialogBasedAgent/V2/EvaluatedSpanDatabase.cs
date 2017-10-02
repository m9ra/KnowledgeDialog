using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class EvaluatedSpanDatabase : SpanDatabase
    {
        private readonly Dictionary<string, NativeEvaluator> _evaluators = new Dictionary<string, NativeEvaluator>();

        internal override IEnumerable<SemanticItem> Query(SemanticItem queryItem)
        {
            var result = new List<SemanticItem>();

            var originalResult = base.Query(queryItem);
            foreach (var item in originalResult)
            {
                if (_evaluators.ContainsKey(item.Answer))
                {
                    var context = new EvaluationContext();
                    var evaluatedItem = _evaluators[item.Answer](context);
                    if (evaluatedItem == null)
                        continue;
                }
                else
                {
                    result.Add(item);
                }
            }

            return result;
        }

        internal void AddEvaluator(string evaluatorId, NativeEvaluator evaluator)
        {
            _evaluators.Add(evaluatorId, evaluator);
            AddSpanElement(evaluatorId);
        }
    }
}
