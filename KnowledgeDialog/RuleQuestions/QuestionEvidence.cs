using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.RuleQuestions
{
    class QuestionEvidence
    {
        /// <summary>
        /// The question which evidence is kept.
        /// </summary>
        public readonly ParsedUtterance Question;

        /// <summary>
        /// How many times we saw positive evidence for the answer.
        /// </summary>
        private readonly Dictionary<NodeReference, int> _positiveEvidence = new Dictionary<NodeReference, int>();

        /// <summary>
        /// How many times we saw negative evidence for the answer.
        /// </summary>
        private readonly Dictionary<NodeReference, int> _negativeEvidence = new Dictionary<NodeReference, int>();

        internal QuestionEvidence(ParsedUtterance question)
        {
            Question = question;
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
            throw new NotImplementedException();
        }

        internal NodeReference GetFeatureNode(NodeReference generalFeatureNode)
        {
            throw new NotImplementedException();
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
