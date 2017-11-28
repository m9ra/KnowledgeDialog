using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class QueryLog
    {
        /// <summary>
        /// Current query.
        /// </summary>
        public readonly SemanticItem Query;

        /// <summary>
        /// Queries done when current log was active.
        /// </summary>
        public IEnumerable<QueryLog> Subqueries => _subqueries;

        public IEnumerable<QueryLog> DependencyQueries => _dependencyQueries;

        public IEnumerable<string> Dependencies => _dependencyQueries.Concat(_subqueries).SelectMany(d => d.Dependencies).Concat(_dependencies).Distinct().ToArray();

        private readonly HashSet<string> _dependencies = new HashSet<string>();

        internal readonly int Id;

        private static int _currentId;

        private bool _hasConditionFailed;

        internal QueryLog Parent { get; private set; }

        internal bool HasConditionFailed => _hasConditionFailed || _dependencyQueries.Any(d => d.HasConditionFailed);

        internal bool ForceHasResult;

        private readonly List<QueryLog> _subqueries = new List<QueryLog>();

        private readonly List<QueryLog> _dependencyQueries = new List<QueryLog>();

        private List<SemanticItem> _result = new List<SemanticItem>();

        internal bool IsDependency;

        internal QueryLog()
        {
            Id = ++_currentId;
        }

        internal QueryLog(SemanticItem query)
            : this()
        {
            Query = query;
        }

        internal void ExtendResult(IEnumerable<SemanticItem> result)
        {
            _result.AddRange(result);
        }

        internal void ReportConditionFail()
        {
            _hasConditionFailed = true;
        }

        internal void AddSubquery(QueryLog subquery)
        {
            if (subquery.Parent != null)
                throw new InvalidOperationException("Cannot reset parent.");

            if (subquery == this)
                throw new InvalidOperationException("Cannot set self as a parent.");

            subquery.Parent = this;

            if (subquery.IsDependency)
            {
                _dependencyQueries.Add(subquery);
            }
            else
            {
                _subqueries.Add(subquery);
            }
        }

        internal void AddDependency(string condition)
        {
            _dependencies.Add(condition);
        }

        internal void RemoveFromParent()
        {
            Parent._subqueries.Remove(this);
            Parent._dependencyQueries.Remove(this);
        }

        internal IEnumerable<SemanticItem> GetQuestions()
        {
            var result = new List<SemanticItem>();

            if (!ForceHasResult && !HasConditionFailed)
            {
                if (_result.Count == 0 && _subqueries.Count == 0)
                    result.Add(Query);
            }

            foreach (var subquery in _subqueries.Concat(_dependencyQueries))
            {
                foreach (var question in subquery.GetQuestions())
                {
                    result.Add(question);
                }
            }

            return result;
        }

        public override string ToString()
        {
            if (Query == null)
                return "root";

            return "Q: " + Query.ToString();
        }

        internal IEnumerable<SemanticItem> ResultWithDependency(string name)
        {
            if (Dependencies.Contains(name))
                return _result;

            return new SemanticItem[0];
        }

    }
}
