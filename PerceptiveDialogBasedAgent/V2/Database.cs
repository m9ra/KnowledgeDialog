using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    public class Database
    {
        private readonly List<SemanticItem> _data = new List<SemanticItem>();

        private readonly HashSet<string> _phraseIndex = new HashSet<string>();

        internal static readonly string YesA = "yes";

        internal static readonly string NoA = "no";

        internal static readonly string IsItTrueQ = "is $@ true ?";

        private Stack<QueryLog> _queryLog = new Stack<QueryLog>();

        internal QueryLog CurrentLogPeek => _queryLog.Peek();

        internal virtual IEnumerable<SemanticItem> Query(SemanticItem queryItem)
        {
            if (queryItem.Question == null)
                throw new NotImplementedException();

            if (queryItem.Answer != null)
                throw new NotImplementedException();

            logPush(queryItem);

            var result = new List<SemanticItem>();
            foreach (var item in _data)
            {
                if (item.Question != queryItem.Question)
                    continue;

                if (item.Constraints.Input != null)
                {
                    var matcher = new InputMatcher();
                    var itemMatches = matcher.Match(item, queryItem).ToArray();

                    var matchedItems = new List<SemanticItem>();
                    foreach (var match in itemMatches)
                    {
                        if (meetConditions(match))
                            matchedItems.Add(match);
                    }

                    result.AddRange(matchedItems.OrderByDescending(i => rank(i)));
                }
                else
                {
                    if (meetConditions(item))
                        result.Add(item);
                }
            }

            logPop(result);

            return result;
        }

        internal void Add(SemanticItem item)
        {
            foreach (var phrase in item.Phrases)
            {
                _phraseIndex.Add(phrase);
            }
            _data.Add(item);
        }

        internal void StartQueryLog()
        {
            if (_queryLog.Count > 0)
                throw new InvalidOperationException("Cannot start log now");

            _queryLog.Push(new QueryLog());
        }

        internal QueryLog FinishLog()
        {
            if (_queryLog.Count != 1)
                throw new InvalidOperationException("Invalid finish state for log");

            return _queryLog.Pop();
        }

        private int rank(SemanticItem item)
        {
            var result = 0;
            foreach (var phrase in item.Phrases)
            {
                if (_phraseIndex.Contains(phrase))
                    result += 1;
            }

            return result;
        }

        private bool meetConditions(SemanticItem item)
        {
            var hasConditions = item.Constraints.Conditions.Any();
            if (!hasConditions)
                return true;

            logPush(item);

            var conditionsResult = true;
            foreach (var condition in item.Constraints.Conditions)
            {
                var constraints = new Constraints().AddInput(condition);
                var queryItem = SemanticItem.AnswerQuery(IsItTrueQ, constraints);

                var result = Query(queryItem).ToArray();
                if (result.Length > 1)
                    throw new NotImplementedException();

                conditionsResult &= result.FirstOrDefault()?.Answer == YesA;
                if (!conditionsResult)
                    break;
            }
            logPop(new SemanticItem[0]);

            return conditionsResult;
        }

        protected void logPush(SemanticItem item)
        {
            if (_queryLog.Count == 0)
                //logging is not enabled
                return;

            var peek = _queryLog.Peek();
            if (peek.Query == item)
            {
                _queryLog.Push(peek);
            }
            else
            {
                var log = new QueryLog(item);
                peek.AddSubquery(log);
                _queryLog.Push(log);
            }
        }

        protected void logPop(IEnumerable<SemanticItem> result)
        {
            var log = _queryLog.Pop();
            log.ExtendResult(result);
        }
    }
}
