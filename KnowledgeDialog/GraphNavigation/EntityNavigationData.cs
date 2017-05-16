using KnowledgeDialog.Dialog.Parsing;
using KnowledgeDialog.Knowledge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.GraphNavigation
{
    [Serializable]
    public class EntityNavigationData
    {
        public readonly string Phrase;

        private readonly NavigationData _parent;

        private readonly object _L_data = new object();

        private readonly VoteContainer<string> _labelHints = new VoteContainer<string>();

        private readonly VoteContainer<Tuple<Edge, EntityInfo>> _relevantPaths = new VoteContainer<Tuple<Edge, EntityInfo>>();

        private readonly VoteContainer<EntityInfo> _relevantEntities = new VoteContainer<EntityInfo>();

        private readonly VoteContainer<EntityInfo> _candidates = new VoteContainer<EntityInfo>();

        internal EntityNavigationData(NavigationData parent, string phrase)
        {
            _parent = parent;
            Phrase = phrase;
        }

        public void AddLabelCandidate(string hint)
        {
            _labelHints.Vote(hint);
        }
    }
}
