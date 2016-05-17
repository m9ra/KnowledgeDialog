using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class RequestContext : StateBase
    {
        public readonly static EdgeIdentifier HasContextAnswerEdge = new EdgeIdentifier();

        private readonly static HashSet<string> _contextIndicators = new HashSet<string>()
        {
            "he","his","him","hisself","himself",
            "she","her","herself",
            "it","its","itself",
            "they","them","themself"            
        };

        protected override ModifiableResponse execute()
        {
            var hasPossibleContext = Context.Pool.ActiveCount > 0;

            /*  if (!hasPossibleContext)
                  Context.SetValue(AcceptAdvice.IsBasedOnContextProperty, StateProperty.FalseValue);

              if (!Context.IsSet(AcceptAdvice.IsBasedOnContextProperty))
                  return Response("I cannot fully understand your question. Are you asking for something connected with your previous question?");*/

            //use heuristic for context determination for now.
            var question = Context.Get(RequestAnswer.QuestionProperty);
            if (question == null)
                question = Context.Get(QuestionAnswering.LastQuestion);

            var isBasedOnContext = hasPossibleContext && hasContextIndicator(question);

            Context.SetValue(AcceptAdvice.IsBasedOnContextProperty, StateProperty2.ToPropertyValue(isBasedOnContext));

            return EmitEdge(HasContextAnswerEdge);
        }

        private bool hasContextIndicator(string question)
        {
            var words = Dialog.UtteranceParser.Parse(question).Words;
            foreach (var word in words)
            {
                if (_contextIndicators.Contains(word))
                    return true;
            }

            return false;
        }
    }
}
