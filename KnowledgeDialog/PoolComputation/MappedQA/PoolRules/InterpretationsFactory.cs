﻿using System;
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
        internal static readonly int MaxSearchWidth = 100;

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

        private readonly HashSet<NodeReference> _visitedNodes = new HashSet<NodeReference>();

        private readonly Stack<PathSegment> _segmentsToVisit = new Stack<PathSegment>();

        internal IEnumerable<NodeReference> SelectedNodes { get { return _selectedNodes; } }

        internal IEnumerable<PoolRuleBase> Rules { get { return _rules; } }

        internal TopicSelector(ComposedGraph graph, NodeReference targetNode)
        {
            _graph = graph;
            _targetNode = targetNode;
        }

        internal bool MoveNext()
        {
            //only at the begining there are no rules available
            var isBegining = _rules.Count == 0;

            _rules.Clear();
            _selectedNodes.Clear();
            if (!_hasMoreRules)
            {
                //there are no more topic rules available
                return false;
            }

            if (isBegining)
            {
                //first simple rule is just inserting the correct answer node.
                _rules.Add(new TransformPoolRule(_targetNode));
                _selectedNodes.Add(_targetNode);

                addChildren(_targetNode, null, _graph);
            }
            else
            {
                if (_segmentsToVisit.Count == 0)
                    //there are no other nodes
                    return false;

                var nextSegment = _segmentsToVisit.Pop();
                addChildren(nextSegment.Node, nextSegment, _graph);

                throw new NotImplementedException("Create interpretation");
            }

            return true;
        }

        private void addChildren(NodeReference node, PathSegment previousSegment, ComposedGraph graph)
        {
            foreach (var edgeTuple in graph.GetNeighbours(node, InterpretationsFactory.MaxSearchWidth))
            {
                var edge = edgeTuple.Item1;
                var isOutcomming = edgeTuple.Item2;
                var child = edgeTuple.Item3;

                if (_visitedNodes.Contains(child))
                    //we have already visited the node
                    continue;

                _segmentsToVisit.Push(new PathSegment(previousSegment, edge, isOutcomming, child));
            }
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
            //there is only one possibility - no other constraints are required.
            var isBeginning = _selectedNodes.Count == 1;

            if (!_hasNextConstraints)
            {
                _rules.Clear();
                _selectedNodes.Clear();
                return false;
            }

            if (_selectedNodes.Count == 1)
            {
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
