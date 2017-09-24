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
        public readonly IEnumerable<QueryLog> Subqueries;

        private readonly List<QueryLog> _subqueries = new List<QueryLog>();

        private SemanticItem[] _result = null;

        internal QueryLog()
        {

        }

        internal QueryLog(SemanticItem query)
        {
            Query = query;
        }

        internal void SetResult(IEnumerable<SemanticItem> result)
        {
            _result = result.ToArray();
        }

        internal void AddSubquery(QueryLog subquery)
        {
            _subqueries.Add(subquery);
        }
    }
}
