using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation;
using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.RuleQuestions
{
    class StructuredInterpretationGenerator
    {
        /// <summary>
        /// The knowledge base.
        /// </summary>
        internal readonly ComposedGraph Graph;

        /// <summary>
        /// Lock for data operations on the generator.
        /// </summary>
        private readonly object _L_Data = new object();

        /// <summary>
        /// Interpretations that are optimized.
        /// </summary>
        private readonly Dictionary<FeatureKey, FeatureEvidence> _featureEvidences = new Dictionary<FeatureKey, FeatureEvidence>();

        /// <summary>
        /// Index of feature covers that are optimized.
        /// </summary>
        private readonly List<FeatureKey> _featureIndex = new List<FeatureKey>();

        /// <summary>
        /// Index of actually optimized feature cover.
        /// </summary>
        private int _actualCoverIndex = 0;

        internal StructuredInterpretationGenerator(ComposedGraph graph)
        {
            Graph = graph;
        }

        /// <summary>
        /// Optimize interpretation in given number of steps.
        /// </summary>
        /// <param name="stepCount">Number of steps.</param>
        internal void Optimize(int stepCount)
        {
            for (var i = 0; i < stepCount; ++i)
            {
                FeatureEvidence optimizedInterpretations;
                lock (_L_Data)
                {
                    var optimizedCover = _featureIndex[_actualCoverIndex];
                    optimizedInterpretations = _featureEvidences[optimizedCover];
                    ++_actualCoverIndex;
                    _actualCoverIndex %= _featureIndex.Count;
                }

                optimizedInterpretations.MakeOptimizationStep();
            }
        }

        internal void AdviceAnswer(string question, NodeReference answer)
        {
            var parsedQuestion = UtteranceParser.Parse(question);
            foreach (var featureCover in FeatureCover.GetFeatureCovers(parsedQuestion, Graph))
            {
                register(featureCover, answer);
            }
        }

        internal IEnumerable<FeatureEvidence> GetEvidences(string question)
        {
            var result = new List<FeatureEvidence>();
            var parsedQuestion = UtteranceParser.Parse(question);
            foreach (var featureCover in FeatureCover.GetFeatureCovers(parsedQuestion, Graph))
            {
                if (_featureEvidences.ContainsKey(featureCover.FeatureKey))
                    result.Add(_featureEvidences[featureCover.FeatureKey]);
            }

            return result;
        }

        private void register(FeatureCover cover, NodeReference answer)
        {
            var evidence = getEvidence(cover);
            evidence.Advice(cover, answer);
        }

        private FeatureEvidence getEvidence(FeatureCover cover)
        {
            FeatureEvidence evidence;
            var key = cover.FeatureKey;
            if (!_featureEvidences.TryGetValue(key, out evidence))
            {
                _featureEvidences[key] = evidence = new FeatureEvidence(cover, Graph);
                _featureIndex.Add(key);
            }

            return evidence;
        }

        internal IEnumerable<NodeReference> Evaluate(string question, StructuredInterpretation structuredInterpretation)
        {
            var matchingCover = getMatchingCover(question, structuredInterpretation.FeatureKey);
            var constraints = structuredInterpretation.GeneralConstraints.Zip(matchingCover.GetInstanceNodes
(Graph), Tuple.Create);

            HashSet<NodeReference> topicNodes = null;
            foreach (var constraint in constraints)
            {
                var constraintSet = constraint.Item1.FindSet(constraint.Item2, Graph);
                if (topicNodes == null)
                    topicNodes = constraintSet;
                else
                    topicNodes.IntersectWith(constraintSet);
            }

            var pool = new ContextPool(Graph);
            pool.Insert(topicNodes.ToArray());

            foreach (var constraint in structuredInterpretation.DisambiguationConstraints)
            {
                constraint.Execute(pool);
            }

            return pool.ActiveNodes;
        }

        private FeatureCover getMatchingCover(string question, FeatureKey key)
        {
            var parsedQuestion = UtteranceParser.Parse(question);
            foreach (var cover in FeatureCover.GetFeatureCovers(parsedQuestion, Graph))
            {
                if (key.Equals(cover.FeatureKey))
                    return cover;
            }

            return null;
        }
    }
}
