using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation
{
    class ActionGroup
    {

        private List<IPoolAction> _actions = new List<IPoolAction>();

        private List<string> _words = new List<string>();

        public IEnumerable<IPoolAction> Actions { get { return _actions; } }

        public IEnumerable<string> RegisteredWords { get { return _words; } }


        internal void Add(string word, IPoolAction action)
        {
            _actions.Add(action);
            _words.Add(word);
        }

        internal bool CanInclude(IPoolAction action)
        {
            var containedAction = Actions.FirstOrDefault();
            if (containedAction == null)
                //group is empty
                return true;

            return containedAction.HasSamePoolEffectAs(action);
        }
    }
}
