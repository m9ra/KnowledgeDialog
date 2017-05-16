using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    [Serializable]
    public class VoteContainer<T>
    {
        private readonly Dictionary<T, Votes> _itemsVotes = new Dictionary<T, Votes>();

        internal void Vote(T item)
        {
            if (!_itemsVotes.TryGetValue(item, out var votes))
                _itemsVotes[item] = votes = new Votes();

            votes.AddPositiveVotes(1);
        }
    }
}
