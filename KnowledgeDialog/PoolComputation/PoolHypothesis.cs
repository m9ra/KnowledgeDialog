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

        public readonly ActionBlock ActionBlock;


        public PoolHypothesis(IEnumerable<KeyValuePair<NodeReference, NodeReference>> substitutions, ActionBlock block)
        {
            Substitutions = substitutions;
            ActionBlock = block;
        }
    }
}
