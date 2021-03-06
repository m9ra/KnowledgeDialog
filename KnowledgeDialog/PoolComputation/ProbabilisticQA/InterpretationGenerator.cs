﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class InterpretationGenerator
    {
        internal readonly IEnumerable<FeatureCover> Covers;

        internal Interpretation ContractedInterpretation { get { return _interpretations.ContractedInterpretation; } }

        private readonly ProbabilisticQAModule _owner;

        private readonly ContextPool _context;

        private readonly InterpretationsFactory _interpretations;

        private readonly TopicSelector _topicSelector;

        private ConstraintSelector _currentConstraintSelector;

        internal InterpretationGenerator(IEnumerable<FeatureCover> covers, InterpretationsFactory interpretations, ContextPool context, ProbabilisticQAModule owner)
        {
            var coversCopy = covers.ToArray();
            if (coversCopy.Length == 0)
                throw new NotSupportedException("Cannot create InterpretationGenerator without feature covers");

            Covers = coversCopy;

            _interpretations = interpretations;
            _context = context.Clone();
            _owner = owner;
            _topicSelector = interpretations.GetTopicSelector(owner.Graph);
            if (_topicSelector.MoveNext())
            {
                //initialize constraint selector
                _currentConstraintSelector = interpretations.GetConstraintSelector(owner.Graph, _topicSelector.SelectedNodes);
            }
        }

        internal Interpretation GetNextInterpretation()
        {
            while (_currentConstraintSelector == null || !_currentConstraintSelector.MoveNext())
            {
                if (!_topicSelector.MoveNext())
                    //there are no other interpretations
                    return null;

                _currentConstraintSelector = _interpretations.GetConstraintSelector(_owner.Graph, _topicSelector.SelectedNodes);
            }

            var interpretation = new Interpretation(_topicSelector.Rules.Concat(_currentConstraintSelector.Rules));
            return interpretation;
        }

        internal ContextPool CreateContextPoolCopy()
        {
            return _context.Clone();
        }

        internal bool ContainsCorrectAnswer(ContextPool contextPool)
        {
            return contextPool.ActiveCount == 1 && contextPool.ContainsInAccumulator(_topicSelector.TargetNode);
        }
    }
}
