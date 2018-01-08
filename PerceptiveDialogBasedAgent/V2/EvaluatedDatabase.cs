using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class EvaluatedDatabase : Database
    {
        private readonly Stack<EvaluationContext> _contextStack = new Stack<EvaluationContext>();

        protected override SemanticItem transformItem(SemanticItem queryItem, SemanticItem resultItem)
        {
            var transformedItem = base.transformItem(queryItem, resultItem);
            var evalutor = getEvaluator(transformedItem.Answer);
            if (evalutor == null)
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
                var evaluationResult = evalutor(evaluationContext);
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

        internal NativeAction GetNativeAction(string nativeActionId)
        {
            foreach (var container in Containers.Reverse())
            {
                var nativeAction = container.GetNativeAction(nativeActionId);
                if (nativeAction != null)
                    return nativeAction;
            }

            return null;
        }
        private NativeEvaluator getEvaluator(string evaluatorName)
        {
            foreach (var container in Containers.Reverse())
            {
                var evalutor = container.GetEvalutor(evaluatorName);
                if (evalutor != null)
                    return evalutor;
            }

            return null;
        }

    }
}
