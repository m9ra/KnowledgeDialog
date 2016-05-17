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
    class QuestionEvidence
    {
        /// <summary>
        /// The question which evidence is kept.
        /// </summary>
        public readonly ParsedUtterance Question;

        /// <summary>
        /// The cover of question features.
        /// </summary>
        public readonly FeatureCover Cover;

        /// <summary>
        /// How many times we saw positive evidence for the answer.
        /// </summary>
        private readonly Dictionary<NodeReference, int> _positiveEvidence = new Dictionary<NodeReference, int>();

        /// <summary>
        /// How many times we saw negative evidence for the answer.
        /// </summary>
        private readonly Dictionary<NodeReference, int> _negativeEvidence = new Dictionary<NodeReference, int>();

        internal QuestionEvidence(FeatureCover questionCover)
        {
            Cover = questionCover;
            Question = questionCover.OriginalUtterance;
        }

        /// <summary>
        /// Gives advice about answer on the question.
        /// </summary>
        /// <param name="answer">The adviced answer.</param>
        internal void Advice(NodeReference answer)
        {
            int evidenceCount;
            _positiveEvidence.TryGetValue(answer, out evidenceCount);
            _positiveEvidence[answer] = evidenceCount + 1;
        }


        /// <summary>
        /// Negation evidence on question's answer.
        /// </summary>
        /// <param name="negatedAnswer">The negated answer.</param>
        internal void Negate(NodeReference negatedAnswer)
        {
            int evidenceCount;
            _negativeEvidence.TryGetValue(negatedAnswer, out evidenceCount);
            _negativeEvidence[negatedAnswer] = evidenceCount + 1;
        }

        internal NodeReference GetBestEvidenceAnswer()
        {
            NodeReference bestAnswer = null;
            var bestEvidence = int.MinValue;
            foreach (var positiveEvidencePair in _positiveEvidence)
            {
                int negativeEvidence;
                _negativeEvidence.TryGetValue(positiveEvidencePair.Key, out negativeEvidence);
                var evidence = positiveEvidencePair.Value - negativeEvidence;

                if (evidence > bestEvidence)
                {
                    bestAnswer = positiveEvidencePair.Key;
                    bestEvidence = evidence;
                }
            }

            return bestAnswer;
        }

        internal NodeReference GetFeatureNode(NodeReference generalFeatureNode, ComposedGraph graph)
        {
            return Cover.GetInstanceNode(generalFeatureNode, graph);
        }

        internal int GetEvidenceScore(NodeReference answer)
        {
            int positiveEvidence, negativeEvidence;

            _positiveEvidence.TryGetValue(answer, out positiveEvidence);
            _negativeEvidence.TryGetValue(answer, out negativeEvidence);

            return positiveEvidence - negativeEvidence;
        }
    }
}
