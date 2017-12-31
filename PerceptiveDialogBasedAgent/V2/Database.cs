using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    public class Database
    {
        private static HashSet<int> _debugTriggers = new HashSet<int>();

        private readonly List<SemanticItem> _data = new List<SemanticItem>();

        private readonly HashSet<string> _phraseIndex = new HashSet<string>();

        private bool _expectDependencyQuery = false;

        private readonly List<EvaluationLogEntry> _evaluationHistory = new List<EvaluationLogEntry>();

        internal const int MaxResolvingDepth = 5;

        private HashSet<string> _span = new HashSet<string>() { YesAnswer, NoAnswer };

        private Stack<QueryLog> _queryLog = new Stack<QueryLog>();

        internal IEnumerable<EvaluationLogEntry> EvaluationHistory => _evaluationHistory;

        internal const string YesAnswer = "yes";

        internal const string NoAnswer = "no";

        internal static readonly string IsItTrueQ = "is $@ true ?";

        internal QueryLog LastQueryLog { get; private set; }

        internal QueryLog CurrentLogPeek => _queryLog.Peek();

        internal QueryLog CurrentLogRoot => _queryLog.Last();

        internal static void DebugTrigger(int id)
        {
            _debugTriggers.Add(id);
        }

        protected virtual SemanticItem transformItem(SemanticItem queryItem, SemanticItem item)
        {
            //nothing to transform by default
            return item;
        }

        /// <summary>
        /// Answers of returned items are in span.
        /// </summary>
        internal IEnumerable<SemanticItem> SpanQuery(SemanticItem queryItem)
        {
            var result = new List<SemanticItem>();

            var itemsToProcess = new Queue<SemanticItem>();
            itemsToProcess.Enqueue(queryItem);
            itemsToProcess.Enqueue(null);

            var depth = 0;
            while (itemsToProcess.Count > 0)
            {
                var item = itemsToProcess.Dequeue();
                if (item == null)
                {
                    itemsToProcess.Enqueue(null);
                    ++depth;
                    if (depth > MaxResolvingDepth)
                        break;

                    continue;
                }

                if (_span.Contains(item.Answer))
                {
                    result.Add(item);
                }
                else
                {
                    SemanticItem newQuery;
                    if (item.Answer == null)
                    {
                        newQuery = item;
                    }
                    else
                    {
                        var newConstraints = queryItem.Constraints.MergeWith(item.Constraints);
                        newConstraints = newConstraints.AddInput(item.Answer);

                        newQuery = SemanticItem.AnswerQuery(queryItem.Question, newConstraints);
                    }

                    var newItems = Query(newQuery).ToArray();
                    if (newItems.Length == 0)
                    {
                        if (item != queryItem)
                            result.Add(item);
                    }
                    foreach (var newItem in newItems)
                    {
                        itemsToProcess.Enqueue(newItem);
                    }
                }
            }

            return result;
        }

        internal IEnumerable<SemanticItem> Query(SemanticItem queryItem)
        {
            var result = new List<SemanticItem>();
            logPush(queryItem);

            if (_debugTriggers.Contains(queryItem.Id))
                queryItem = queryItem;

            var matchingEntries = fetchMatchingEntries(queryItem);
            foreach (var matchingEntry in matchingEntries)
            {
                var resultEntry = transformItem(queryItem, matchingEntry);
                if (resultEntry != null)
                    result.Add(resultEntry);
            }

            logPop(result);
            _evaluationHistory.Add(new EvaluationLogEntry(queryItem.Constraints.Input, queryItem.Question, result));

            return result;
        }

        internal void AddSpanElement(string spanAnswer)
        {
            _span.Add(spanAnswer);
        }

        private IEnumerable<SemanticItem> fetchMatchingEntries(SemanticItem queryItem)
        {
            if (queryItem.Question == null)
                throw new NotImplementedException();

            if (queryItem.Answer != null)
                throw new NotImplementedException();

            var result = new List<SemanticItem>();
            if (queryItem.Question == IsItTrueQ)
            {
                //TODO conditions should be more general thing
                var inputConditions = new HashSet<string>(queryItem.Constraints.Conditions);
                var condition = queryItem.InstantiateInputWithEntityVariables();
                logDependency(condition);
                if (inputConditions.Contains(condition))
                {
                    //condition is met because of input
                    result.Add(SemanticItem.Entity(YesAnswer));
                }

                var negatedCondition = Constraints.Negate(condition);
                if (inputConditions.Contains(negatedCondition))
                {
                    //we have got a negative result
                    result.Add(SemanticItem.Entity(NoAnswer));
                }
            }

            if (result.Count > 0)
                return result;

            foreach (var item in _data.Reverse<SemanticItem>())
            {
                if (item.Question != queryItem.Question)
                    continue;

                /*if ((item.Constraints.Input == null) != (queryItem.Constraints == null))
                    //when input is provided it should be considered
                    continue;*/

                if (item.Constraints.Input != null)
                {
                    var matcher = new InputMatcher();
                    var itemMatches = matcher.Match(item, queryItem).ToArray();

                    var matchedItems = new List<SemanticItem>();
                    foreach (var match in itemMatches)
                    {
                        if (meetConditions(queryItem, match))
                            matchedItems.Add(match);
                    }

                    result.AddRange(matchedItems.OrderByDescending(i => rank(i)));
                }
                else
                {
                    if (meetConditions(queryItem, item))
                        result.Add(item);
                }

                if (result.Count > 0)
                    break;
            }

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

        private bool meetConditions(SemanticItem queryItem, SemanticItem item)
        {
            var hasConditions = item.Constraints.Conditions.Any();
            if (!hasConditions)
                return true;

            //logPush(item);

            var conditionsResult = true;
            foreach (var condition in item.Constraints.Conditions)
            {
                logDependency(condition);
                var constraints = queryItem.Constraints.AddInput(condition);
                var conditionQueryItem = SemanticItem.AnswerQuery(IsItTrueQ, constraints);

                expectDependencyQuery();
                var result = SpanQuery(conditionQueryItem).ToArray();

                conditionsResult &= result.LastOrDefault()?.Answer == YesAnswer;
                if (!conditionsResult)
                {
                    if (result.FirstOrDefault()?.Answer == NoAnswer)
                    {
                        logFailedCondition();
                        //logRemoveTopSubquery();
                    }

                    break;
                }
            }
            // logPop(new SemanticItem[0]);

            return conditionsResult;
        }

        private void expectDependencyQuery()
        {
            _expectDependencyQuery = true;
        }

        private void logDependency(string condition)
        {
            if (_queryLog.Count == 0)
                //logging is not enabled
                return;

            _queryLog.Peek().AddDependency(condition);
        }


        private void logFailedCondition()
        {
            if (_queryLog.Count == 0)
                //logging is not enabled
                return;

            _queryLog.Peek().Parent.ReportConditionFail();
        }


        private void logRemoveTopSubquery()
        {
            if (_queryLog.Count == 0)
                //logging is not enabled
                return;

            _queryLog.Peek().RemoveFromParent();
        }

        protected void logPush(SemanticItem item)
        {
            if (_queryLog.Count == 0)
                //logging is not enabled
                return;

            Log.QueryPush(item);

            var peek = _queryLog.Peek();
            if (peek.Query == item)
            {
                _queryLog.Push(peek);
            }
            else
            {
                var log = new QueryLog(item);
                if (_expectDependencyQuery)
                {
                    log.IsDependency = true;
                    _expectDependencyQuery = false;
                }

                peek.AddSubquery(log);
                _queryLog.Push(log);
            }
        }

        protected void logHasResult()
        {
            if (_queryLog.Count == 0)
                //loging is not enabled
                return;

            _queryLog.Peek().ForceHasResult = true;
        }

        protected void logPop(IEnumerable<SemanticItem> result)
        {
            if (_queryLog.Count == 0)
                //loging is not enabled
                return;

            Log.QueryPop(result);

            var log = _queryLog.Pop();
            LastQueryLog = log;
            log.ExtendResult(result);
        }
    }
}
