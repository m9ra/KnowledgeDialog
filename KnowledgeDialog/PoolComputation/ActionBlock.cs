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

        internal readonly KnowledgeClassifier<bool> OutputFilter;

        internal ActionBlock(ComposedGraph graph,params IPoolAction[] actions)
            : this(graph,(IEnumerable<IPoolAction>)actions)
        {
            
        }


        internal ActionBlock(ComposedGraph graph,IEnumerable<IPoolAction> actions)
        {
            OutputFilter = new KnowledgeClassifier<bool>(graph);
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
