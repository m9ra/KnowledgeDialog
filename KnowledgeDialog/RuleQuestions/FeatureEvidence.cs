using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation;
using KnowledgeDialog.PoolComputation.ProbabilisticQA;

using KnowledgeDialog.PoolComputation.MappedQA;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;
using KnowledgeDialog.PoolComputation.MappedQA.Features;


namespace KnowledgeDialog.RuleQuestions
{
    class FeatureEvidence
    {
        public static readonly int PathMaxWidth = 20;

        public static readonly int PathMaxLength = 20;

        public static readonly int MaxPathCount = 10;

        public static readonly int InterpretationCountLimit = 5;

        /// <summary>
        /// Feature cover which interpretations are kept here.
        /// </summary>
        public readonly FeatureKey FeatureKey;

        /// <summary>
        /// Knowledge graph for interpretation generation.
        /// </summary>
        public readonly ComposedGraph Graph;

        /// <summary>
        /// Evidence of question instance to answers.
        /// </summary>
        private readonly Dictionary<ParsedUtterance, QuestionEvidence> _questions = new Dictionary<ParsedUtterance, QuestionEvidence>();

        private readonly Dictionary<NodeReference, HashSet<NodeReference>> _generalNodesToDomains = new Dictionary<NodeReference, HashSet<NodeReference>>();

        /// <summary>
        /// Interpretations available for <see cref="FeatureKey"/>. Interpretation nodes are mapped to nodes in <see cref="FeatureKey"/>.
        /// </summary>
        private readonly List<Ranked<StructuredInterpretation>> _availableInterpretations = new List<Ranked<StructuredInterpretation>>();

        /// <summary>
        /// Generator for structured topics.
        /// </summary>
        private StructuredTopicGenerator _topicGenerator;

        /// <summary>
        /// Queue of constraint generators each for feature cover instance.
        /// </summary>
        private readonly Queue<ConstraintSelector> _constraintGenerators = new Queue<ConstraintSelector>();

        private readonly NodeReference[] _generalFeatureNodes;
        public IEnumerable<Ranked<StructuredInterpretation>> AvailableRankedInterpretations { get { return _availableInterpretations; } }

        internal FeatureEvidence(FeatureCover cover, ComposedGraph graph)
        {
            Graph = graph;
            FeatureKey = cover.FeatureKey;
            _generalFeatureNodes = cover.GetGeneralNodes(graph).ToArray();
        }

        private HashSet<NodeReference> findDomain(NodeReference generalNode)
        {
            //TODO this has to be refactored out
            var domains = new HashSet<NodeReference>();
            foreach (var question in _questions.Values)
            {
                var featureNode = question.GetFeatureNode(generalNode, Graph);
                domains.UnionWith(Graph.GetForwardTargets(new[] { featureNode }, new[] { Edge.Incoming("en.label"), Edge.Incoming("P31") }));
            }

          /*  var edges = Graph.GetNeighbours(new NodeReference("Q30"),100).ToArray();*/
            return domains;
        }

        /// <summary>
        /// Gives advice about answer on the question.
        /// </summary>
        /// <param name="question">The adviced question.</param>
        /// <param name="answer">The adviced answer.</param>
        public void Advice(FeatureCover question, NodeReference answer)
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
        public void Negate(FeatureCover question, NodeReference negatedAnswer)
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
        /// <param name="selector">The generator.</param>
        private void addNewInterpretation(ConstraintSelector selector)
        {
            var interpretation = getInterpretation(selector);
            var rankedInterpretation = rank(interpretation);

            if (rankedInterpretation.Rank == 0)
                throw new NotSupportedException("This should not happen");

            //insertion sort
            var isInserted = false;
            for (var i = 0; i < _availableInterpretations.Count; ++i)
            {
                if (_availableInterpretations[i].Rank < rankedInterpretation.Rank)
                {
                    _availableInterpretations.Insert(i, rankedInterpretation);
                    isInserted = true;
                    break;
                }
            }
            if (!isInserted)
                _availableInterpretations.Add(rankedInterpretation);

            //remove exceeding interpretations if needed
            if (_availableInterpretations.Count > InterpretationCountLimit)
                _availableInterpretations.RemoveRange(InterpretationCountLimit, _availableInterpretations.Count - InterpretationCountLimit);
        }

        private StructuredInterpretation getInterpretation(ConstraintSelector selector)
        {
            var constraints = selector.Rules;
            var interpretation = new StructuredInterpretation(FeatureKey, _topicGenerator.TopicConstraints, constraints);

            return interpretation;
        }

        private Ranked<StructuredInterpretation> rank(StructuredInterpretation interpretation)
        {
            var evidenceScore = 0.0;
            foreach (var question in _questions)
            {
                var answer = evaluate(interpretation, question.Value);
                if (answer.Count() != 1)
                    //this is not good answer
                    continue;

                var answerNode = answer.First();
                evidenceScore += question.Value.GetEvidenceScore(answerNode);
            }
            evidenceScore += generalizationScore(interpretation);
            return new Ranked<StructuredInterpretation>(interpretation, evidenceScore);
        }

        private double generalizationScore(StructuredInterpretation interpretation)
        {
            HashSet<NodeReference> topicNodes = null;
            for (var i = 0; i < _generalFeatureNodes.Length; ++i)
            {
                var generalNode = _generalFeatureNodes[i];
                var constraint = interpretation.GetGeneralConstraint(i);

                var nodeDomain = getInstanceDomain(generalNode);
                var topicalizedDomain = getConstrainedLayer(nodeDomain, constraint);

                if (topicNodes == null)
                    topicNodes = new HashSet<NodeReference>();
                else
                    topicNodes.IntersectWith(nodeDomain);
            }

            var pool = new ContextPool(Graph);
            pool.Insert(topicNodes.ToArray());
            foreach (var constraint in interpretation.DisambiguationConstraints)
            {
                constraint.Execute(pool);
            }

            var topicNodesCount = topicNodes.Count + 1;
            var generalizationBonus = 1.0 * pool.ActiveCount / topicNodesCount;
            var generalizationRatio = 1 - 1.0 / topicNodesCount;

            return generalizationRatio * generalizationBonus;
        }

        private IEnumerable<NodeReference> getConstrainedLayer(IEnumerable<NodeReference> nodeDomain, KnowledgeConstraint constraint)
        {
            return Graph.GetForwardTargets(nodeDomain, constraint.Path);
        }

        private IEnumerable<NodeReference> getInstanceDomain(NodeReference generalNode)
        {
            return _generalNodesToDomains[generalNode];
        }

        private IEnumerable<NodeReference> evaluate(StructuredInterpretation interpretation, QuestionEvidence question)
        {
            HashSet<NodeReference> inputSet = null;
            for (var i = 0; i < _generalFeatureNodes.Length; ++i)
            {
                var generalNode = _generalFeatureNodes[i];
                var featureNode = question.GetFeatureNode(generalNode, Graph);
                var constraint = interpretation.GetGeneralConstraint(i);
                if (inputSet == null)
                {
                    inputSet = constraint.FindSet(featureNode, Graph);
                }
                else
                {
                    inputSet.IntersectWith(constraint.FindSet(featureNode, Graph));
                }
            }

            var pool = new ContextPool(Graph);
            pool.Insert(inputSet.ToArray());
            foreach (var distinguishConstraint in interpretation.DisambiguationConstraints)
            {
                distinguishConstraint.Execute(pool);
            }
            return pool.ActiveNodes;
        }

        /// <summary>
        /// Fills queue with constraint generators on new topic.
        /// </summary>
        private void fillGeneratorQueue()
        {
            if (!_topicGenerator.MoveNext())
                //there are no more topics
                return;

            foreach (var evidence in _questions.Values)
            {
                var bestEvidenceAnswer = evidence.GetBestEvidenceAnswer();
                var nodeMapping = new List<NodeReference>();
                foreach (var generalFeatureNode in getGeneralFeatureNodes())
                {
                    var mappedNode = evidence.GetFeatureNode(generalFeatureNode, Graph);
                    nodeMapping.Add(mappedNode);
                }

                //initialize generators for each question instance
                var constraintGenerator = _topicGenerator.InitializeSelector(nodeMapping, bestEvidenceAnswer);
                if (constraintGenerator != null)
                    _constraintGenerators.Enqueue(constraintGenerator);
            }
        }

        /// <summary>
        /// Gets evidence for given question.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <returns>The evidence.</returns>
        private QuestionEvidence getQuestionEvidence(FeatureCover question)
        {
            QuestionEvidence evidence;
            if (!_questions.TryGetValue(question.OriginalUtterance, out evidence))
                _questions[question.OriginalUtterance] = evidence = new QuestionEvidence(question);
            return evidence;
        }

        /// <summary>
        /// Updates interpretations based on new evidence.
        /// Can results in current interpretation invalidation.
        /// </summary>
        private void checkInterpretationUpdates()
        {
            //TODO update could be done efficiently

            foreach (var generalNode in _generalFeatureNodes)
            {
                var domain = findDomain(generalNode);
                _generalNodesToDomains[generalNode] = domain;
            }

            _availableInterpretations.Clear();
            _topicGenerator = null;

            var mappedConstraints = findEvidenceBestConstraintOptions();
            _topicGenerator = new StructuredTopicGenerator(mappedConstraints, Graph);

            _constraintGenerators.Clear();
            fillGeneratorQueue();
        }

        private IEnumerable<KnowledgeConstraintOptions> findEvidenceBestConstraintOptions()
        {
            var result = new List<KnowledgeConstraintOptions>();
            foreach (var generalFeatureNode in getGeneralFeatureNodes())
            {
                //find all possible paths spotted by feature answer nodes
                var featureNodeGeneralPaths = new HashSet<KnowledgeConstraint>();
                foreach (var evidence in _questions.Values)
                {
                    var bestEvidenceAnswer = evidence.GetBestEvidenceAnswer();
                    var featureNode = evidence.GetFeatureNode(generalFeatureNode, Graph);

                    var evidencePaths = getConstraints(generalFeatureNode, featureNode, bestEvidenceAnswer);

                    featureNodeGeneralPaths.UnionWith(evidencePaths);
                }

                result.Add(selectBestConstraints(generalFeatureNode, featureNodeGeneralPaths));
            }

            return result;
        }

        private IEnumerable<KnowledgeConstraint> getConstraints(NodeReference generalNode, NodeReference featureNode, NodeReference bestEvidenceAnswer)
        {
            var paths = Graph.GetPaths(featureNode, bestEvidenceAnswer, PathMaxLength, PathMaxWidth).Take(MaxPathCount).ToArray();
            var result = new List<KnowledgeConstraint>();
            foreach (var path in paths)
            {
                result.Add(new KnowledgeConstraint(path));
            }

            return result;
        }

        private IEnumerable<NodeReference> getGeneralFeatureNodes()
        {
            return _generalFeatureNodes;
        }

        private KnowledgeConstraintOptions selectBestConstraints(NodeReference generalNode, HashSet<KnowledgeConstraint> constraints)
        {
            var topConstraints = findTopEvidenceConstraints(generalNode, constraints);
            return new KnowledgeConstraintOptions(topConstraints);
        }

        private IEnumerable<KnowledgeConstraint> findTopEvidenceConstraints(NodeReference generalNode, IEnumerable<KnowledgeConstraint> constraints)
        {
            var orderedConstraints = (from constraint in constraints select constraint).OrderByDescending(c => getEvidenceScore(generalNode, c)).ToArray();
            var selectedConstraints = orderedConstraints.Take(1).ToArray();

            var scores = new List<int>();
            foreach (var constraint in selectedConstraints)
            {
                var score = getEvidenceScore(generalNode, constraint);
                scores.Add(score);
            }

            return selectedConstraints;
        }

        private int getEvidenceScore(NodeReference generalNode, KnowledgeConstraint constraint)
        {
            var evidenceScore = 0;
            foreach (var evidence in _questions.Values)
            {
                var featureNode = evidence.GetFeatureNode(generalNode, Graph);
                var answer = evidence.GetBestEvidenceAnswer();

                if (constraint.IsSatisfiedBy(featureNode, answer, Graph))
                    //if answer is possible under given constraint, we count it to the score
                    evidenceScore += evidence.GetEvidenceScore(answer);
            }

            return evidenceScore;
        }
    }
}
