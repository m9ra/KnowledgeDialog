using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class EvaluatedDatabase : Database
    {
        private readonly Dictionary<string, NativeEvaluator> _evaluators = new Dictionary<string, NativeEvaluator>();

        private readonly Stack<EvaluationContext> _contextStack = new Stack<EvaluationContext>();

        protected override SemanticItem transformItem(SemanticItem queryItem, SemanticItem resultItem)
        {
            var transformedItem = base.transformItem(queryItem, resultItem);
            if (!_evaluators.ContainsKey(transformedItem.Answer))
                //keep the item without changes
                return transformedItem;

            if (_contextStack.Count == 0)
            {
                //push root context
                _contextStack.Push(new EvaluationContext(null, this, queryItem));
            }

            var evaluationContext = new EvaluationContext(_contextStack.Peek(), this, transformedItem);
            _contextStack.Push(evaluationContext);
            try
            {
                var evaluationResult = _evaluators[transformedItem.Answer](evaluationContext);
                return evaluationResult;
            }
            finally
            {
                _contextStack.Pop();
                if (_contextStack.Count == 1)
                    //root context is popped out
                    _contextStack.Pop();
            }
        }

        internal void AddEvaluator(string evaluatorId, NativeEvaluator evaluator)
        {
            _evaluators.Add(evaluatorId, evaluator);
            AddSpanElement(evaluatorId);
        }
    }
}
