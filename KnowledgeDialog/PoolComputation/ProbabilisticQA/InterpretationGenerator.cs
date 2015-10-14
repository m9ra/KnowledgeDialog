using System;
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

        private readonly ProbabilisticQAModule _owner;

        private readonly InterpretationsFactory _interpretations;

        private readonly TopicSelector _topicSelector;

        private ConstraintSelector _currentConstraintSelector;

        internal InterpretationGenerator(IEnumerable<FeatureCover> covers, InterpretationsFactory interpretations, ProbabilisticQAModule owner)
        {
            var coversCopy = covers.ToArray();
            if (coversCopy.Length == 0)
                throw new NotSupportedException("Cannot create InterpretationGenerator without feature covers");

            Covers = coversCopy;

            _interpretations = interpretations;
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
            if (_topicSelector.SelectedNodesCount == 1)
                //TODO strategy for better topic selection
                _topicSelector.MoveNext();

            while (!_currentConstraintSelector.MoveNext())
            {
                if (!_topicSelector.MoveNext())
                    //there are no other interpretations
                    return null;

                _currentConstraintSelector = _interpretations.GetConstraintSelector(_owner.Graph, _topicSelector.SelectedNodes);
            }

            return new Interpretation(_topicSelector.Rules.Concat(_currentConstraintSelector.Rules));
        }

    }
}
