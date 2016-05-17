using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class QuestionAnsweringAction : MachineActionBase
    {
        internal static double AnswerConfidenceThreshold = 0.9;

        internal static double EquivalenceConfidenceThreshold = 0.5;

        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return InputState.HasNonAnsweredQuestion;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            var answer = getAnswer(InputState.Question);
            if (answer != null)
            {
                //we have answer, let present it
                EmitResponse(answer);
                RemoveQuestion();
                return;
            }

            var equivalenceCandidate = getEquivalenceCandidate(InputState.Question);
            if (equivalenceCandidate != null)
            {
                //we don't know how to answer the question
                //however we know some similar question
                SetEquivalenceCandidate(equivalenceCandidate);

                //state has to be processed further - we are forwarding
                //control into another machine action
                ForwardControl();
                return;
            }

            //We don't know the answer - we let the state to be processed further
            SetQuestionAsUnknown();
            ForwardControl();
        }

        private string getAnswer(ParsedUtterance question)
        {
            var answerHypothesis = InputState.QA.GetBestHypothesis(question);
            if (answerHypothesis == null)
                //we don't know anything about the question
                return null;

            if (answerHypothesis.Control.Score < AnswerConfidenceThreshold)
                //sentences are not similar enough
                return null;

            var answerNodes = InputState.QA.GetAnswer(answerHypothesis);
            if (answerNodes == null)
                //cannot find the node
                return null;

            var joinedAnswer = string.Join(" and ", answerNodes.Select(a => a.Data));
            return string.Format("It is {0}.", joinedAnswer);
        }

        private ParsedUtterance getEquivalenceCandidate(ParsedUtterance question)
        {
            var bestHypothesis = InputState.QA.GetBestHypothesis(question);
            if (bestHypothesis == null)
                return null;

            var score = bestHypothesis.Control.Score;
            if (score > AnswerConfidenceThreshold || score < EquivalenceConfidenceThreshold)
                //no equivalence here
                return null;

            var equivalenceCandidate = substitute(bestHypothesis.Control.ParsedSentence, question);
            if (equivalenceCandidate == null)
                //we are not able to expose equivalence question
                return null;

            return equivalenceCandidate;
        }

        private ParsedUtterance substitute(ParsedUtterance pattern, ParsedUtterance utterance)
        {
            var patternNodes = InputState.QA.GetPatternNodes(pattern);
            var utteranceNodes = InputState.QA.GetRelatedNodes(utterance).ToArray();

            if (patternNodes.Count != utteranceNodes.Length)
                //semantic equivalence is missing - we won't allow equivalence question
                return null;

            //substitute every word in pattern, according to utterance
            var substitutions = HeuristicQAModule.GetSubstitutions(utteranceNodes, patternNodes, InputState.QA.Graph);
            var substitutedWords = new List<string>();
            foreach (var patternWord in pattern.Words)
            {
                var patternNode = InputState.QA.Graph.GetNode(patternWord);

                NodeReference substitutedNode;
                if (!substitutions.TryGetValue(patternNode, out substitutedNode))
                    //there is no substitution - by default we take node from pattern
                    substitutedNode = patternNode;

                substitutedWords.Add(substitutedNode.Data);
            }

            return ParsedUtterance.From(substitutedWords);
        }
    }
}