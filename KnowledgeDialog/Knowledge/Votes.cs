using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    [Serializable]
    public class Votes
    {
        private int _totalVotes;

        private int _positiveVotes;

        public int Positive => _positiveVotes;

        public int Total => _totalVotes;

        public void AddPositiveVotes(int votes)
        {
            _positiveVotes += votes;
            _totalVotes += Math.Abs(votes);
        }
    }
}
