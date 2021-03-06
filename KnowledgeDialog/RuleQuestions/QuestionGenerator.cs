﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.RuleQuestions
{
    class NodeQuestion
    {
        public readonly double Impact = 0;

        public readonly ParsedUtterance OriginalQuestion = null;

        public readonly ParsedUtterance ActualQuestion = null;

        public readonly NodeReference QuestionedAnswer = null;
    }

    class TripletRelevanceQuestion
    {
        public readonly double Impact = 0;

        public readonly ParsedUtterance OriginalQuestion = null;

        public readonly NodeReference Subject = null;

        public readonly Edge Relation = null;

        public readonly NodeReference Object = null;
    }

    class QuestionGenerator
    {
        private readonly StructuredInterpretationGenerator _generator;

        internal QuestionGenerator(StructuredInterpretationGenerator generator)
        {
            _generator = generator;
        }

        public IEnumerable<NodeQuestion> FindDistinguishingNodeQuestions(FeatureEvidence evidence)
        {
            var substitutions = findUnknownSubstitutions(evidence);
            foreach (var substitution in substitutions)
            {
                //compute different answers that we might receive
                //notice that answers which evidence is known are not included
                var answerCounts = getPossibleAnswerCounts(substitution, evidence);

                //choose the most believed one
                yield return generateDistinguishingQuestion(answerCounts, evidence);
            }
        }

        public IEnumerable<NodeQuestion> FindEvidenceNodeQuestions(FeatureEvidence evidence)
        {
            throw new NotImplementedException();
        }

        private Dictionary<NodeReference, int> getPossibleAnswerCounts(NodeReference[] substitution, FeatureEvidence evidence)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<NodeReference[]> findUnknownSubstitutions(FeatureEvidence evidence)
        {
            throw new NotImplementedException();
        }

        private NodeQuestion generateDistinguishingQuestion(Dictionary<NodeReference, int> answerCounts, FeatureEvidence evidence)
        {
            throw new NotImplementedException();
        }
    }
}
