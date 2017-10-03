using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class SpanDatabase : Database
    {
        internal readonly int MaxResolvingDepth = 5;

        protected HashSet<string> _span = new HashSet<string>() { YesA, NoA };

        internal override IEnumerable<SemanticItem> Query(SemanticItem queryItem)
        {
            return query(queryItem, MaxResolvingDepth);
        }

        private IEnumerable<SemanticItem> query(SemanticItem queryItem, int resolvingDepth)
        {
            logPush(queryItem);

            var result = new List<SemanticItem>();
            foreach (var resultItem in base.Query(queryItem))
            {
                var isInSpan = _span.Contains(resultItem.Answer);

                if (resolvingDepth == 0 || isInSpan)
                {
                    result.Add(resultItem);
                }
                else
                {
                    var constraints = resultItem.Constraints.AddInput(resultItem.Answer);
                    //TODO think about queryItem constraints
                    var newQuery = SemanticItem.AnswerQuery(queryItem.Question, constraints);

                    result.AddRange(query(newQuery, resolvingDepth - 1));
                }
            }
            logPop(result);
            return result;
        }


        internal void AddSpanElement(string evaluatorId)
        {
            _span.Add(evaluatorId);
        }
    }
}
