using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class ActionBlock
    {
        internal readonly IEnumerable<IPoolAction> Actions;

        internal readonly IEnumerable<NodeReference> RequiredSubstitutions;

        internal ActionBlock(params IPoolAction[] actions)
            : this((IEnumerable<IPoolAction>)actions)
        {
        }


        internal ActionBlock(IEnumerable<IPoolAction> actions)
        {
            var nodes = new List<NodeReference>();
            foreach (var action in actions)
            {
                if (action.SemanticOrigin == null)
                    continue;

                var substitutedNode = action.SemanticOrigin.StartNode;
                if (nodes.Contains(substitutedNode))
                    continue;


                nodes.Add(substitutedNode);
            }

            Actions = actions.ToArray();
            RequiredSubstitutions = nodes.ToArray();
        }

    }
}
