using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;


namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class FeaturedRule
    {
        internal readonly FeatureBase Feature;

        internal readonly ContextRule Rule;

        internal FeaturedRule(FeatureBase feature, ContextRule rule)
        {
            Feature = feature;
            Rule = rule;
        }
    }

    class OptimizedEntry
    {
        private readonly FeatureCover[] _covers;

        private readonly InterpretationsFactory _interpretations;

        private readonly FeatureMapping _originMapping;

        private readonly TopicSelector _topicSelector;

        private ConstraintSelector _currentConstraintSelector;

        internal Interpretation CurrentInterpretation { get; private set; }

        internal IEnumerable<FeatureCover> Covers { get { return _covers; } }

        internal NodeReference CorrectAnswerNode { get { return _interpretations.CorrectAnswerNode; } }

        internal OptimizedEntry(FeatureMapping originMapping, InterpretationsFactory interpretations, IEnumerable<FeatureCover> covers)
        {
            _covers = covers.ToArray();
            if (_covers.Length == 0)
                throw new NotSupportedException("Cannot create optimized entry without feature covers");

            _interpretations = interpretations;
            _originMapping = originMapping;

            _topicSelector = interpretations.GetTopicSelector(originMapping.Graph);
            if (_topicSelector.MoveNext())
            {
                _currentConstraintSelector = interpretations.GetConstraintSelector(originMapping.Graph, _topicSelector.SelectedNodes);
                Next();
            }
        }

        internal FeatureInstance GetSimpleFeatureInstance()
        {
            return _interpretations.GetSimpleFeatureInstance();
        }

        /// <summary>
        /// Sets new constraint and selector pattern.
        /// </summary>
        /// <returns></returns>
        internal bool Next()
        {
            CurrentInterpretation = null;

            while (!_currentConstraintSelector.MoveNext())
            {
                if (!_topicSelector.MoveNext())
                    //there are no other interpretations
                    return false;

                _currentConstraintSelector = _interpretations.GetConstraintSelector(_originMapping.Graph, _topicSelector.SelectedNodes);
            }

            CurrentInterpretation = new Interpretation(_topicSelector.Rules.Concat(_currentConstraintSelector.Rules));
            return true;
        }
    }
}
