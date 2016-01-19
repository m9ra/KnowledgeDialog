using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.RuleQuestions
{
    class StructuredInterpretationGenerator
    {
        /// <summary>
        /// Lock for data operations on the generator.
        /// </summary>
        private readonly object _L_Data = new object();

        /// <summary>
        /// Interpretations that are optimized.
        /// </summary>
        private readonly Dictionary<FeatureCover, FeatureEvidence> _featureInterpretations = new Dictionary<FeatureCover, FeatureEvidence>();

        /// <summary>
        /// Index of feature covers that are optimized.
        /// </summary>
        private readonly List<FeatureCover> _coverIndex = new List<FeatureCover>();

        /// <summary>
        /// Index of actually optimized feature cover.
        /// </summary>
        private int _actualCoverIndex = 0;

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
                    var optimizedCover = _coverIndex[_actualCoverIndex];
                    optimizedInterpretations = _featureInterpretations[optimizedCover];
                    ++_actualCoverIndex;
                    _actualCoverIndex %= _coverIndex.Count;
                }

                optimizedInterpretations.MakeOptimizationStep();
            }
        }
    }
}
