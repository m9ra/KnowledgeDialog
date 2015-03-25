using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class PoolHypothesis
    {
        public readonly IEnumerable<KeyValuePair<NodeReference, NodeReference>> Substitutions;

        public readonly IEnumerable<IPoolAction> Actions;


        public PoolHypothesis(IEnumerable<KeyValuePair<NodeReference, NodeReference>> substitutions, IEnumerable<IPoolAction> actions)
        {
            Substitutions = substitutions;
            Actions = new List<IPoolAction>(actions);
        }

        internal void Suggest(NodeReference suggestedAnswer)
        {
            throw new NotImplementedException();
        }
    }
}
