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

        private readonly List<QueryLog> _subqueries = new List<QueryLog>();

        private List<SemanticItem> _result = new List<SemanticItem>();

        internal QueryLog()
        {

        }

        internal QueryLog(SemanticItem query)
        {
            Query = query;
        }

        internal void ExtendResult(IEnumerable<SemanticItem> result)
        {
            _result.AddRange(result);
        }

        internal void AddSubquery(QueryLog subquery)
        {
            _subqueries.Add(subquery);
        }

        internal IEnumerable<SemanticItem> GetQuestions()
        {
            var result = new List<SemanticItem>();
            foreach (var subquery in _subqueries)
            {
                if (subquery._result.Count == 0 && subquery._subqueries.Count == 0)
                {
                    result.Add(subquery.Query);
                }
                else
                {
                    result.AddRange(subquery.GetQuestions());
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
    }
}
