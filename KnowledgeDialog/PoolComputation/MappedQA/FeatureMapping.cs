using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA
{
    class RulesDistribution
    {
        private List<ContextRule> _rules = new List<ContextRule>();

        public int Size { get { return _rules.Count; } }

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
            _rules.Add(contextRule);
        }
    }

    class FeatureMapping
    {
        private readonly Dictionary<FeatureBase, RulesDistribution> _featureToDistributions = new Dictionary<FeatureBase, RulesDistribution>();

        private readonly List<OptimizedEntry> _optimizedEntries = new List<OptimizedEntry>();

        /// <summary>
        /// <remarks>
        /// Every feauture with zero count has to be removed.
        /// </remarks>
        /// </summary>
        private readonly Dictionary<FeatureBase, int> _registeredFeatures = new Dictionary<FeatureBase, int>();

        /// <summary>
        /// <remarks>
        /// Every rule part with zero count has to be removed.
        /// </remarks>
        /// </summary>
        private readonly Dictionary<RulePart, int> _registeredRuleParts = new Dictionary<RulePart, int>();

        /// <summary>
        /// Every pair with zero count has to be removed.
        /// </summary>
        private readonly Dictionary<Tuple<RulePart, FeatureBase>, int> _registeredRulePartFeaturePairs = new Dictionary<Tuple<RulePart, FeatureBase>, int>();

        /// <summary>
        /// Knowledge graph used for mapping.
        /// </summary>
        internal readonly ComposedGraph Graph;

        /// <summary>
        /// Constant that is used for stabilization of math operations.
        /// </summary>
        internal readonly double StabilizationConstant = 0.0001;

        /// <summary>
        /// How much of heuristic probability is used for feature and rule mapping.
        /// </summary>
        internal readonly double HeuristicProbabilityWeight = 0.2;

        internal FeatureMapping(ComposedGraph graph)
        {
            Graph = graph;
        }

        internal IEnumerable<Ranked<ContextRuleMapping>> GetRankedMappings(FeatureCover cover)
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
            var entry = new OptimizedEntry(this, interpretationsFactory, covers);
            registerEntry(entry);
        }

        internal void Optimize()
        {
            var hasChange = false;
            do
            {
                foreach (var entry in _optimizedEntries)
                {
                    var rankedInterpretation = getNextRankedInterpretation(entry);

                    var entryRankedRules = new List<Ranked<ContextRuleMapping>>();
                    foreach (var cover in entry.Covers)
                    {
                        //score, how good are features matching to the rules
                        var rankedMapping = getRankedMapping(cover, rankedInterpretation);
                        entryRankedRules.Add(rankedMapping);
                    }

                    foreach (var rankedRule in entryRankedRules)
                    {
                        if (tryImprove(rankedRule))
                            //improvement was successful
                            hasChange = true;
                    }
                }
            } while (hasChange);
        }

        #region Optimization routines

        private Ranked<Interpretation> getNextRankedInterpretation(OptimizedEntry entry)
        {
            if (!entry.Next())
                //there are no more patterns available
                return null;

            //get next interpretation
            var interpretation = entry.CurrentInterpretation;

            //rank according to generalization point of view
            var similarAnswerNodes = getSimilarNodes(entry.CorrectAnswerNode);
            var substitutedAnswerNodes = getSubstitutedAnswers(interpretation);
            var similarSubstitutions = similarAnswerNodes.Intersect(substitutedAnswerNodes).ToArray();
            var generalizationAbility = 1.0 * similarSubstitutions.Length / similarAnswerNodes.Count;
            return new Ranked<Interpretation>(interpretation, generalizationAbility);
        }

        private Ranked<ContextRuleMapping> getRankedMapping(FeatureCover cover, Ranked<Interpretation> rankedInterpretation)
        {
            var ruleDecomposition = createInterpretationDecompositions(rankedInterpretation.Value);
            //assign rule parts to features
            var assignments = new Dictionary<RulePart, FeatureBase>();
            foreach (var rulePart in ruleDecomposition.Parts)
            {
                var bestFeature = getBestFeature(rulePart, cover);
                assignments[rulePart] = bestFeature;
            }

            var rankedMapping = createRankedContextRuleMapping(assignments);

            //rerank with interpretation ranking
            var rerankedMapping = new Ranked<ContextRuleMapping>(rankedMapping.Value, rankedMapping.Rank * rankedInterpretation.Rank);
            return rerankedMapping;
        }

        private FeatureBase getBestFeature(RulePart rulePart, FeatureCover cover)
        {
            FeatureInstance bestFeatureInstance = null;
            var bestPMI = double.NegativeInfinity;
            foreach (var featureInstance in cover.FeatureInstances)
            {
                //select best feature according to pointwise mutual information
                var featurePMI = getPointwiseMutualInformation(rulePart, featureInstance.Feature);
                if (featurePMI > bestPMI)
                {
                    bestPMI = featurePMI;
                    bestFeatureInstance = featureInstance;
                }
            }

            return bestFeatureInstance.Feature;
        }

        private double getPointwiseMutualInformation(RulePart rulePart, FeatureBase feature)
        {
            var ruleProbability = P(rulePart);
            var featureProbability = P(feature);
            var coocurenceProbability = P(rulePart, feature);

            return Math.Log(coocurenceProbability / (ruleProbability * featureProbability + StabilizationConstant) + StabilizationConstant);
        }

        private InterpretationDecomposition createInterpretationDecompositions(Interpretation interpretation)
        {
            var ruleParts = new List<RulePart>();
            foreach (var rule in interpretation.Rules)
            {
                ruleParts.AddRange(rule.Parts);
            }

            return new InterpretationDecomposition(ruleParts);
        }

        private Ranked<ContextRuleMapping> createRankedContextRuleMapping(Dictionary<RulePart, FeatureBase> assignments)
        {
            var partClusters = getPartClusters(assignments);
            var referencedPartClusters = getReferencedPartClusters(partClusters);

            var featuresToRules = new Dictionary<FeatureBase, ContextRule>();
            foreach (var referencedPart in referencedPartClusters)
            {
                var contextRule = createContextRule(referencedPart);
                featuresToRules.Add(referencedPart.Feature, contextRule);
            }

            var mapping = new ContextRuleMapping(featuresToRules);
            return Rank(mapping);
        }

        private IEnumerable<PartCluster> getPartClusters(Dictionary<RulePart, FeatureBase> assignments)
        {
            var featureToParts = new Dictionary<FeatureBase, List<RulePart>>();
            foreach (var featurePair in assignments)
            {
                var feature = featurePair.Value;
                var part = featurePair.Key;

                List<RulePart> parts;
                if (!featureToParts.TryGetValue(feature, out parts))
                    featureToParts[feature] = parts = new List<RulePart>();

                parts.Add(part);
            }

            var clusters = new List<PartCluster>();
            foreach (var feature in featureToParts.Keys)
            {
                var partCluster = new PartCluster(feature, featureToParts[feature]);
                clusters.Add(partCluster);
            }

            return clusters;
        }

        private IEnumerable<ReferencedPartCluster> getReferencedPartClusters(IEnumerable<PartCluster> partClusters)
        {
            var referencedPartClusters = new List<ReferencedPartCluster>();

            foreach (var partCluster in partClusters)
            {
                var referencedPartCluster = new ReferencedPartCluster(partCluster);

                referencedPartClusters.Add(referencedPartCluster);
            }

            return referencedPartClusters;
        }

        private ContextRule createContextRule(ReferencedPartCluster referencedPartCluster)
        {
            throw new NotImplementedException();
        }

        internal Ranked<ContextRuleMapping> Rank(ContextRuleMapping mapping)
        {
            throw new NotImplementedException();
        }

        private bool tryImprove(Ranked<ContextRuleMapping> mapping)
        {
            throw new NotImplementedException();
        }

        private HashSet<NodeReference> getSimilarNodes(NodeReference node)
        {
            return new HashSet<NodeReference>(Graph.GetForwardTargets(new[] { node }, new[] { Tuple.Create(Graph.IsEdge, true), Tuple.Create(Graph.IsEdge, false) }));
        }

        private HashSet<NodeReference> getSubstitutedAnswers(Interpretation interpretation)
        {
            //TODO find alternative answers - it is required for generalization measure
            return new HashSet<NodeReference>();
        }

        private double P(RulePart rule)
        {
            var allPartsCount = _registeredRuleParts.Count;
            int ruleCount;
            _registeredRuleParts.TryGetValue(rule, out ruleCount);

            return 1.0 * ruleCount / (1 + allPartsCount);
        }

        private double P(FeatureBase feature)
        {
            var allFeatureCount = _registeredFeatures.Count;
            int featureCount;
            _registeredFeatures.TryGetValue(feature, out featureCount);

            return 1.0 * featureCount / (1 + allFeatureCount);
        }

        private double P(RulePart rule, FeatureBase feature)
        {
            int coocurenceCount;
            int featureCount;
            _registeredFeatures.TryGetValue(feature, out featureCount);
            _registeredRulePartFeaturePairs.TryGetValue(Tuple.Create(rule, feature), out coocurenceCount);

            var pModel = 1.0 * coocurenceCount / (1 + featureCount);

            var heuristicModel = P_Heuristic(rule, feature);
            return HeuristicProbabilityWeight * heuristicModel + (1.0 - HeuristicProbabilityWeight) * pModel;
        }

        private double P_Heuristic(RulePart rule, FeatureBase feature)
        {
            return feature.Probability(rule);
        }

        #endregion


        private void registerEntry(OptimizedEntry entry)
        {
            var initialInstance = entry.GetSimpleFeatureInstance();
            var initialRule = new FeaturedRule(initialInstance.Feature, new ContextRule(new CompositionPoolRule(entry.CurrentInterpretation.Rules)));


            RulesDistribution distribution;
            if (!_featureToDistributions.TryGetValue(initialRule.Feature, out distribution))
                _featureToDistributions[initialRule.Feature] = distribution = new RulesDistribution();

            distribution.Add(initialRule.Rule);
            _optimizedEntries.Add(entry);
        }

        private RulesDistribution[] getDistributions(FeatureCover cover)
        {
            var distributions = new List<RulesDistribution>();
            foreach (var instance in cover.FeatureInstances)
            {
                RulesDistribution distribution;
                _featureToDistributions.TryGetValue(instance.Feature, out distribution);
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
