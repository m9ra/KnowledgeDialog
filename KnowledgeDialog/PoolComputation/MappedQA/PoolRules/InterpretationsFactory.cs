using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class InterpretationsFactory
    {
        internal readonly NodeReference CorrectAnswerNode;

        private Dialog.ParsedUtterance parsedQuestion;
        private bool isBasedOnContext;
        private IEnumerable<NodeReference> context;

        public InterpretationsFactory(Dialog.ParsedUtterance parsedQuestion, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            // TODO: Complete member initialization
            this.parsedQuestion = parsedQuestion;
            this.isBasedOnContext = isBasedOnContext;
            this.CorrectAnswerNode = correctAnswerNode;
            this.context = context;
        }

        internal FeatureInstance GetSimpleFeatureInstance()
        {
            return SimpleFeatureGenerator.CreateSimpleFeatureInstance(parsedQuestion);
        }

        internal TopicSelector GetTopicSelector(ComposedGraph graph)
        {
            return new TopicSelector(graph, CorrectAnswerNode);
        }

        internal ConstraintSelector GetConstraintSelector(ComposedGraph graph, IEnumerable<NodeReference> selectedNodes)
        {
            return new ConstraintSelector(graph, CorrectAnswerNode, selectedNodes);
        }
    }

    class TopicSelector
    {
        private bool _hasMoreRules = true;

        private readonly ComposedGraph _graph;

        private readonly NodeReference _targetNode;

        private readonly List<PoolRuleBase> _rules = new List<PoolRuleBase>();

        private readonly HashSet<NodeReference> _selectedNodes = new HashSet<NodeReference>();

        internal IEnumerable<NodeReference> SelectedNodes { get { return _selectedNodes; } }

        internal IEnumerable<PoolRuleBase> Rules { get { return _rules; } }

        internal TopicSelector(ComposedGraph graph, NodeReference targetNode)
        {
            _graph = graph;
            _targetNode = targetNode;
        }

        internal bool MoveNext()
        {

            _rules.Clear();
            _selectedNodes.Clear();
            if (!_hasMoreRules)
            {
                //there are no more topic rules available
                return false;
            }

            //only at the begining there are no rules available
            var isBegining = _rules.Count == 0;
            if (isBegining)
            {
                //first simple rule is just inserting the correct answer node.
                _rules.Add(new TransformPoolRule(_targetNode));
                _selectedNodes.Add(_targetNode);
            }
            else
            {
                throw new NotImplementedException();
            }

            return true;
        }
    }

    class ConstraintSelector
    {
        private bool _hasNextConstraints = true;

        private readonly ComposedGraph _graph;

        private readonly NodeReference _targetNode;

        private readonly HashSet<NodeReference> _selectedNodes = new HashSet<NodeReference>();

        private readonly List<PoolRuleBase> _rules = new List<PoolRuleBase>();

        internal IEnumerable<PoolRuleBase> Rules { get { return _rules; } }

        internal ConstraintSelector(ComposedGraph graph, NodeReference targetNode, IEnumerable<NodeReference> selectedNodes)
        {
            _graph = graph;
            _targetNode = targetNode;
            _selectedNodes.UnionWith(selectedNodes);

            if (!_selectedNodes.Contains(_targetNode))
                throw new NotSupportedException("Cannot constraint nodes without target");

        }

        internal bool MoveNext()
        {
            if (!_hasNextConstraints)
            {
                _rules.Clear();
                return false;
            }

            if (_selectedNodes.Count == 1)
            {
                //there is only one possibility - no other constraints are required.
                _hasNextConstraints = false;
            }
            else
            {
                throw new NotImplementedException("Create complex constraints");
            }

            return true;
        }
    }
}
