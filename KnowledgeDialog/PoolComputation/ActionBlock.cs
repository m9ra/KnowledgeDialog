using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation.PoolActions;

namespace KnowledgeDialog.PoolComputation
{
    class ActionBlock
    {
        private List<IPoolAction> _actions = new List<IPoolAction>();

        internal IEnumerable<IPoolAction> Actions { get { return _actions; } }

        internal NodesEnumeration RequiredSubstitutions { get; private set; }

        internal readonly KnowledgeClassifier<bool> OutputFilter;

        internal readonly string OriginalSentence;

        internal ActionBlock(ComposedGraph graph, params IPoolAction[] actions)
            : this(graph, (IEnumerable<IPoolAction>)actions)
        {

        }

        internal ActionBlock(ComposedGraph graph, IEnumerable<IPoolAction> actions)
        {
            var semantics= actions.FirstOrDefault();
            if (semantics != null && semantics.SemanticOrigin != null)
            {
                OriginalSentence = Dialog.UtteranceParser.Parse(semantics.SemanticOrigin.Utterance).OriginalSentence;
            }

            OutputFilter = new KnowledgeClassifier<bool>(graph);
            _actions.AddRange(actions);
            RequiredSubstitutions = findRequiredSubstitutions(actions);
        }


        internal void UpdatePush(IEnumerable<PushAction> pushActions)
        {
            _actions.RemoveAll(action => action is PushAction);
            _actions.AddRange(pushActions);

            var previousCount = RequiredSubstitutions.Count;
            RequiredSubstitutions = findRequiredSubstitutions(_actions);
            if (previousCount != RequiredSubstitutions.Count)
                throw new NotSupportedException("Invalid update");
        }

        internal void UpdateInsert(IEnumerable<InsertAction> insertActions)
        {
            _actions.RemoveAll(action => action is InsertAction);
            _actions.AddRange(insertActions);


            var previousCount = RequiredSubstitutions.Count;
            RequiredSubstitutions = findRequiredSubstitutions(_actions);
            if (previousCount != RequiredSubstitutions.Count)
                throw new NotSupportedException("Invalid update");
        }

        private NodesEnumeration findRequiredSubstitutions(IEnumerable<IPoolAction> actions)
        {
            var nodes = new List<NodeReference>();
            foreach (var action in Actions)
            {
                if (action.SemanticOrigin == null)
                    continue;

                var substitutedNode = action.SemanticOrigin.StartNode;
                if (nodes.Contains(substitutedNode))
                    continue;

                nodes.Add(substitutedNode);
            }

            return new NodesEnumeration(nodes.ToArray());
        }
    }
}
