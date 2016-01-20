using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;
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
    }
}
