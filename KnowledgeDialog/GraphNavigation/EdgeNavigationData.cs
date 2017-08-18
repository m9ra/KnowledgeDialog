using KnowledgeDialog.Knowledge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.GraphNavigation
{
    [Serializable]
    public class EdgeNavigationData
    {
        public readonly string Edge;

        private readonly NavigationData _container;

        private readonly VoteContainer<string> _edgeExpressions = new VoteContainer<string>();

        public IEnumerable<Tuple<string, int>> ExpressionVotes => _edgeExpressions.ItemsVotes.Select(p => Tuple.Create(p.Key, p.Value.Positive));

        internal EdgeNavigationData(NavigationData container, string edge)
        {
            _container = container;
            Edge = edge;
        }

        public void AddExpression(string expression)
        {
            _edgeExpressions.Vote(expression);
            _container.Save();
        }
    }
}
