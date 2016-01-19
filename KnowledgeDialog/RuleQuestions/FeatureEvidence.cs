using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.ProbabilisticQA;

using KnowledgeDialog.PoolComputation.MappedQA;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;
using KnowledgeDialog.PoolComputation.MappedQA.Features;


namespace KnowledgeDialog.RuleQuestions
{
    class FeatureEvidence
    {
        /// <summary>
        /// Feature cover which interpretations are kept here.
        /// </summary>
        public readonly FeatureCover Cover;

        /// <summary>
        /// Knowledge graph for interpretation generation.
        /// </summary>
        public readonly ComposedGraph Graph;

        /// <summary>
        /// Evidence of question instance to answer.s
        /// </summary>
        private readonly Dictionary<ParsedUtterance, QuestionEvidence> _evidence = new Dictionary<ParsedUtterance, QuestionEvidence>();

        /// <summary>
        /// Interpretations available for <see cref="Cover"/>. Interpretation nodes are mapped to nodes in <see cref="Cover"/>.
        /// </summary>
        private readonly List<Interpretation> _availableInterpretations = new List<Interpretation>();

        /// <summary>
        /// Generator for structured topics.
        /// </summary>
        private StructuredTopicGenerator _topicGenerator;

        /// <summary>
        /// Queue of constraint generators each for feature cover instance.
        /// </summary>
        private readonly Queue<ConstraintSelector> _constraintGenerators = new Queue<ConstraintSelector>();

        private readonly NodeReference[] _generalFeatureNodes;

        internal FeatureEvidence(FeatureCover cover, ComposedGraph graph)
        {
            Cover = cover;
            _generalFeatureNodes = cover.GetNodes(graph).ToArray();
        }

        /// <summary>
        /// Gives advice about answer on the question.
        /// </summary>
        /// <param name="question">The adviced question.</param>
        /// <param name="answer">The adviced answer.</param>
        public void Advice(ParsedUtterance question, NodeReference answer)
        {
            var evidence = getQuestionEvidence(question);
            evidence.Advice(answer);

            checkInterpretationUpdates();
        }

        /// <summary>
        /// Negation evidence on question's answer.
        /// </summary>
        /// <param name="question">The adviced question.</param>
        /// <param name="negatedAnswer">The negated answer.</param>
        public void Negate(ParsedUtterance question, NodeReference negatedAnswer)
        {
            var evidence = getQuestionEvidence(question);
            evidence.Negate(negatedAnswer);

            checkInterpretationUpdates();
        }

        internal bool MakeOptimizationStep()
        {
            while (!_topicGenerator.IsEnd || _constraintGenerators.Count > 0)
            {
                if (_constraintGenerators.Count == 0)
                {
                    //all generators on the topic has been fully processed
                    fillGeneratorQueue();
                    continue;
                }

                var currentGenerator = _constraintGenerators.Dequeue();
                if (currentGenerator.MoveNext())
                {
                    //queue the generator back for next processing
                    _constraintGenerators.Enqueue(currentGenerator);
                    addNewInterpretation(currentGenerator);
                    return true;
                }
            }

            //there are no other interpretations
            return false;
        }

        /// <summary>
        /// Adds new interpretation based on given generator.
        /// </summary>
        /// <param name="currentGenerator">The generator.</param>
        private void addNewInterpretation(ConstraintSelector currentGenerator)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fills queue with constraint generators on new topic.
        /// </summary>
        private void fillGeneratorQueue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets evidence for given question.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <returns>The evidence.</returns>
        private QuestionEvidence getQuestionEvidence(ParsedUtterance question)
        {
            QuestionEvidence evidence;
            if (!_evidence.TryGetValue(question, out evidence))
                _evidence[question] = evidence = new QuestionEvidence(question);
            return evidence;
        }

        /// <summary>
        /// Updates interpretations based on new evidence.
        /// Can results in current interpretation invalidation.
        /// </summary>
        private void checkInterpretationUpdates()
        {
            //TODO update could be done efficiently
            _availableInterpretations.Clear();
            _topicGenerator = null;

            var mappedConstraints = findEvidenceBestConstraintOptions();
            _topicGenerator = new StructuredTopicGenerator(mappedConstraints, Graph);

            fillGeneratorQueue();
        }

        private IEnumerable<KnowledgeConstraintOptions> findEvidenceBestConstraintOptions()
        {
            var generalConstraints = new List<HashSet<KnowledgeConstraint>>();
            foreach (var generalFeatureNode in getGeneralFeatureNodes())
            {
                //find all possible paths spotted by feature answer nodes
                var featureNodeGeneralPaths = new HashSet<KnowledgeConstraint>();
                generalConstraints.Add(featureNodeGeneralPaths);
                foreach (var evidence in _evidence.Values)
                {
                    var bestEvidenceAnswer = evidence.GetBestEvidenceAnswer();
                    var featureNode = evidence.GetFeatureNode(generalFeatureNode);

                    var evidencePaths = getGeneralConstraints(featureNode, bestEvidenceAnswer);

                    featureNodeGeneralPaths.UnionWith(evidencePaths);

                }
            }

            return selectBestConstraints(generalConstraints);
        }

        private IEnumerable<KnowledgeConstraint> getGeneralConstraints(NodeReference featureNode, NodeReference bestEvidenceAnswer)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<NodeReference> getGeneralFeatureNodes()
        {
            return _generalFeatureNodes;
        }

        private IEnumerable<KnowledgeConstraintOptions> selectBestConstraints(List<HashSet<KnowledgeConstraint>> generalConstraints)
        {
            var result = new List<KnowledgeConstraintOptions>();
            foreach (var constraintSet in generalConstraints)
            {
                var topConstraints = findTopEvidenceConstraints(constraintSet);
                result.Add(new KnowledgeConstraintOptions(topConstraints));
            }
            return result;
        }

        private IEnumerable<KnowledgeConstraint> findTopEvidenceConstraints(IEnumerable<KnowledgeConstraint> constraints)
        {
            var orderedConstraints = (from constraint in constraints select constraint).OrderByDescending(c => getEvidenceScore(c)).ToArray();
            return orderedConstraints.Take(5).ToArray();
        }

        private int getEvidenceScore(KnowledgeConstraint constraint)
        {
            var evidenceScore = 0;
            var generalNode = constraint.Node;
            foreach (var evidence in _evidence.Values)
            {
                var featureNode = evidence.GetFeatureNode(generalNode);
                var answer = evidence.GetBestEvidenceAnswer();

                if (constraint.IsSatisfiedBy(featureNode, answer, Graph))
                    //if answer is possible under given constraint, we count it to the score
                    evidenceScore += evidence.GetEvidenceScore(answer);
            }

            return evidenceScore;
        }
    }
}
