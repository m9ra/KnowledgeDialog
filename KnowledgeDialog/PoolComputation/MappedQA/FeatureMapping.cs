using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA
{
    class RulesDistribution
    {
        public int Size { get; set; }

        internal ContextRule GetRule(int ruleIndex)
        {
            throw new NotImplementedException();
        }

        internal double GetProbability(int ruleIndex)
        {
            throw new NotImplementedException();
        }

        internal void Add(ContextRule contextRule)
        {
            throw new NotImplementedException();
        }
    }

    class FeatureMapping
    {
        private readonly Dictionary<FeatureBase, RulesDistribution> _featureToDistributions = new Dictionary<FeatureBase, RulesDistribution>();

        private readonly List<OptimizedEntry> _optimizedEntries = new List<OptimizedEntry>();

        internal IEnumerable<ScoredRuleMapping> GetScoredMappings(FeatureCover cover)
        {
            var distributions = getDistributions(cover);

            var currentLayer = new List<RuleChain>();
            var newLayer = new List<RuleChain>();

            //compute chains of distributions in layers
            currentLayer.Add(new RuleChain());
            foreach (var distribution in distributions)
            {
                for (var ruleIndex = 0; ruleIndex < distribution.Size; ++ruleIndex)
                {
                    if (distribution == null)
                        throw new NotImplementedException("What to do, when distribution is not available?");

                    var rule = distribution.GetRule(ruleIndex);
                    var probability = distribution.GetProbability(ruleIndex);

                    foreach (var ruleChain in currentLayer)
                    {
                        newLayer.Add(ruleChain.ExtendBy(rule, probability));
                    }
                }

                //efficiently swap layers
                var swappedLayer = currentLayer;
                currentLayer = newLayer;
                newLayer = swappedLayer;
                newLayer.Clear();
            }
            throw new NotImplementedException("Select only valid rules");
        }

        internal void Add(InterpretationsFactory interpretationsFactory, IEnumerable<FeatureCover> covers)
        {
            //create simple mapping which doesn't need expensive computation
            var entry = new OptimizedEntry(interpretationsFactory, covers);
            registerEntry(entry);
        }

        private void registerEntry(OptimizedEntry entry)
        {
            foreach (var featuredRule in entry.CurrentFeaturedRules)
            {
                RulesDistribution distribution;
                if (!_featureToDistributions.TryGetValue(featuredRule.Feature, out distribution))
                    _featureToDistributions[featuredRule.Feature] = distribution = new RulesDistribution();

                distribution.Add(featuredRule.Rule);
            }
        }

        private RulesDistribution[] getDistributions(FeatureCover cover)
        {
            var distributions = new List<RulesDistribution>();
            foreach (var feature in cover.Features)
            {
                RulesDistribution distribution;
                _featureToDistributions.TryGetValue(feature, out distribution);
                distributions.Add(distribution);
            }

            return distributions.ToArray();
        }
    }

    /// <summary>
    /// Chain of rules with assigned probability.
    /// </summary>
    class RuleChain
    {
        /// <summary>
        /// Previous chain instance.
        /// </summary>
        private readonly RuleChain _previousChain;

        /// <summary>
        /// Rule of current part of chain.
        /// </summary>
        private readonly ContextRule _rule;

        /// <summary>
        /// Complete probability of the chain (including previous parts of chain).
        /// </summary>
        private readonly double _totalProbability;

        /// <summary>
        /// Initializes empty chain.
        /// </summary>
        internal RuleChain()
        {
            _totalProbability = 1.0;
            _previousChain = null;
            _rule = null;
        }

        /// <summary>
        /// Initializes part of chain based on previous parts of chain.
        /// </summary>
        private RuleChain(RuleChain previousChain, ContextRule rule, double totalProbability)
        {
            _previousChain = previousChain;
            _rule = rule;
            _totalProbability = totalProbability;
        }

        /// <summary>
        /// Extends current chain about new rule with given probability.
        /// </summary>
        /// <param name="rule">Extending rule.</param>
        /// <param name="probability">Probability of extending rule.</param>
        /// <returns>Extended chain.</returns>
        internal RuleChain ExtendBy(ContextRule rule, double probability)
        {
            return new RuleChain(this, rule, _totalProbability * probability);
        }
    }
}
