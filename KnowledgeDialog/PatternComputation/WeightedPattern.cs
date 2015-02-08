using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PatternComputation.Actions;


using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    class WeightedPattern
    {
        /// <summary>
        /// Inputs of "knowledge perceptron".
        /// </summary>
        private readonly Dictionary<PathFeature, double> _weightedFeatures = new Dictionary<PathFeature, double>();

        /// <summary>
        /// Scaling factor of current pattern.
        /// </summary>
        public double Scale { get; internal set; }

        /// <summary>
        /// Features that are used by pattern.
        /// </summary>
        public IEnumerable<PathFeature> Features { get { return _weightedFeatures.Keys; } }

        /// <summary>
        /// Group that was created in context of current pattern.
        /// </summary>
        public readonly KnowledgeGroup ContextGroup;

        /// <summary>
        /// Action that is associated with the pattern
        /// </summary>
        public readonly ActionBase Action;

        public WeightedPattern(KnowledgeGroup group, ActionBase action)
        {
            Action = action;
            ContextGroup = group;
            var features = group.Features;

            //set initial weights
            var featureWeight = 1.0 / features.Count();
            foreach (var feature in features)
            {
                _weightedFeatures[feature] = featureWeight;
            }
        }

        public double GetWeight(PathFeature feature)
        {
            return _weightedFeatures[feature];
        }

        /// <summary>
        /// Decrease feature weights according to delta.
        /// </summary>
        /// <param name="threshold"></param>
        /// <param name="evaluation"></param>
        internal void ChangeWeights(double delta, EvaluationContext evaluation)
        {
            var sortedFeatures = _weightedFeatures.OrderByDescending(w => w.Value * evaluation.GetScore(w.Key));

            var missingFeatureDecrease = Math.Abs(delta);
            var decreasedFeatures = new List<KnowledgePath>();
            foreach (var sortedFeature in sortedFeatures)
            {
                _weightedFeatures[sortedFeature.Key] = 0;

                var strength = evaluation.GetScore(sortedFeature.Key);
                var weight = sortedFeature.Value;
                missingFeatureDecrease -= strength * weight;

                if (missingFeatureDecrease < 0)
                    break;
            }

            var totalWeight = _weightedFeatures.Values.Sum();
            if (totalWeight == 0)
                throw new NotImplementedException("Cannot normalize feature weights");

            foreach (var feature in _weightedFeatures.Keys.ToArray())
            {
                _weightedFeatures[feature] = _weightedFeatures[feature] / totalWeight;
            }
        }
    }
}
