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

        private Stack<QueryLog> _queryLog = new Stack<QueryLog>();

        internal IEnumerable<SemanticItem> Query(SemanticItem queryItem)
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

                    foreach (var match in itemMatches)
                    {
                        if (meetConditions(match))
                            result.Add(match);
                    }
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

        private bool meetConditions(SemanticItem item)
        {
            foreach (var condition in item.Constraints.Conditions)
                throw new NotImplementedException();

            return true;
        }

        private void logPush(SemanticItem item)
        {
            if (_queryLog.Count == 0)
                //logging is not enabled
                return;

            var log = new QueryLog(item);
            _queryLog.Peek().AddSubquery(log);
            _queryLog.Push(log);
        }

        private void logPop(IEnumerable<SemanticItem> result)
        {
            var log = _queryLog.Pop();
            log.SetResult(result);
        }
    }
}
