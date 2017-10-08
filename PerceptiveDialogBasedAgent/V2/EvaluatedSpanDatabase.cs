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

        private Stack<EvaluationContext> _contextStack = new Stack<EvaluationContext>();

        internal override IEnumerable<SemanticItem> Query(SemanticItem queryItem)
        {
            logPush(queryItem);

            if (_contextStack.Count == 0)
                _contextStack.Push(new EvaluationContext(null, this, queryItem));

            var result = new List<SemanticItem>();

            var originalResult = base.Query(queryItem);
            foreach (var item in originalResult)
            {
                if (_evaluators.ContainsKey(item.Answer))
                {
                    var context = new EvaluationContext(_contextStack.Peek(), this, item);
                    try
                    {
                        _contextStack.Push(context);
                        var evaluatedItem = _evaluators[item.Answer](context);
                        if (evaluatedItem == null)
                            continue;

                        if (_span.Contains(evaluatedItem.Answer))
                        {
                            result.Add(evaluatedItem);
                            continue;
                        }

                        var spanQueryItem = SemanticItem.AnswerQuery(queryItem.Question, queryItem.Constraints.AddInput(evaluatedItem.Answer));
                        var spannedItems = Query(spanQueryItem);
                        if (spannedItems.Any())
                            result.AddRange(spannedItems);
                        else
                            result.Add(evaluatedItem);
                    }
                    finally
                    {
                        _contextStack.Pop();
                    }

                }
                else
                {
                    result.Add(item);
                }
            }

            if (_contextStack.Count == 1)
                _contextStack.Pop();

            logPop(result);

            return result;
        }

        internal void AddEvaluator(string evaluatorId, NativeEvaluator evaluator)
        {
            _evaluators.Add(evaluatorId, evaluator);
            AddSpanElement(evaluatorId);
        }
    }
}
