using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog;

using KnowledgeDialog.PoolComputation.PoolActions;

using KnowledgeDialog.Database;

namespace KnowledgeDialog.PoolComputation
{
    public class HeuristicQAModule : QuestionAnsweringModuleBase
    {
        private readonly Dictionary<string, QuestionEntry> _questions = new Dictionary<string, QuestionEntry>();

        private readonly Dictionary<string, List<QuestionEntry>> _questionsPatternIndex = new Dictionary<string, List<QuestionEntry>>();

        internal readonly UtteranceMapping<ActionBlock> Triggers;

        internal static readonly int MaximumGraphDepth = 10;

        internal static readonly int MaximumGraphWidth = 1000;

        public HeuristicQAModule(ComposedGraph graph, CallStorage storage)
            : base(graph, storage)
        {
            Triggers = new UtteranceMapping<ActionBlock>(graph);
        }

        #region Template implementation
        protected override bool adviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            if (question == null || question.Trim() == "")
                return false;

            fillPool(context);
            var questionEntry = GetQuestionEntry(UtteranceParser.Parse(question));
            questionEntry.RegisterAnswer(isBasedOnContext, correctAnswerNode);

            return
                updateOldActions(question, isBasedOnContext, correctAnswerNode) ||
                createNewActions(question, isBasedOnContext, correctAnswerNode);
        }

        protected override void repairAnswer(string question, NodeReference suggestedAnswer, IEnumerable<NodeReference> context)
        {
            fillPool(context);
            var hypotheses = GetSortedHypotheses(UtteranceParser.Parse(question)).ToArray();

            foreach (var hypothesis in hypotheses)
            {
                var actualAnswer = getActualAnswer(hypothesis);
                var isCorrect = actualAnswer.Contains(suggestedAnswer);

                hypothesis.Control.Suggest(isCorrect);
            }

            if (suggestedAnswer != null)
            {
                var isBasedOnContext = hypotheses.Length > 0 && hypotheses[0].ActionBlock.Actions.Any(a => a is ExtendAction);
                AdviceAnswer(question, isBasedOnContext, suggestedAnswer);
            }
        }

        protected override void setEquivalence(string patternQuestion, string queriedQuestion, bool isEquivalent)
        {
            var parsedQuestion = UtteranceParser.Parse(patternQuestion);

            if (isEquivalent)
            {
                var bestMap = Triggers.BestMap(parsedQuestion);
                if (bestMap == null)
                    return;

                Triggers.SetMapping(queriedQuestion, bestMap.Value);
            }
            else
            {
                Triggers.DisableEquivalence(patternQuestion, queriedQuestion);
            }
        }

        protected override void negate(string question)
        {
            var parsedSentence = UtteranceParser.Parse(question);
            var bestHypothesis = GetBestHypothesis(parsedSentence);
            if (bestHypothesis == null)
                //we cannot learn anything
                return;

            var currentAnswer = getActualAnswer(bestHypothesis);
            foreach (var answer in currentAnswer)
            {
                bestHypothesis.ActionBlock.OutputFilter.Advice(answer, false, false);
            }

            bestHypothesis.ActionBlock.OutputFilter.Retrain();
        }


        #endregion

        public IEnumerable<NodeReference> GetAnswer(ParsedUtterance question)
        {
            var bestHypothesis = GetBestHypothesis(question);
            return GetAnswer(bestHypothesis);
        }

        internal IEnumerable<NodeReference> GetAnswer(PoolHypothesis bestHypothesis)
        {
            if (bestHypothesis == null)
                return new NodeReference[0];

            var pool = Pool.Clone();

            var substitutions = bestHypothesis.Substitutions;
            var block = bestHypothesis.ActionBlock;
            runActions(pool, block, substitutions);

            return pool.ActiveNodes;
        }

        internal NodesEnumeration GetPatternNodes(ParsedUtterance sentence)
        {
            //if (!_questions.ContainsKey(sentence.OriginalSentence))
            //    throw new KeyNotFoundException("GetPatternNodes: " + sentence.OriginalSentence);

            var entry = GetQuestionEntry(sentence);

            return entry.QuestionNodes;
        }

        internal QuestionEntry GetQuestionEntry(ParsedUtterance question)
        {
            QuestionEntry entry;
            if (!_questions.TryGetValue(question.OriginalSentence, out entry))
            {
                _questions[question.OriginalSentence] = entry = new QuestionEntry(question.OriginalSentence, Graph);
                var pattern = getPatternQuestion(entry);

                List<QuestionEntry> patternQuestions;
                if (!_questionsPatternIndex.TryGetValue(pattern, out patternQuestions))
                    _questionsPatternIndex[pattern] = patternQuestions = new List<QuestionEntry>();

                patternQuestions.Add(entry);
            }

            return entry;
        }

        internal PoolHypothesis GetBestHypothesis(ParsedUtterance question)
        {
            return GetSortedHypotheses(question).FirstOrDefault();
        }

        internal IEnumerable<PoolHypothesis> GetSortedHypotheses(ParsedUtterance utterance)
        {
            var scoredActions = Triggers.FindMapping(utterance);
            var availableNodes = GetRelatedNodes(utterance).ToArray();

            var result = new List<PoolHypothesis>();
            foreach (var scoredAction in scoredActions)
            {
                var substitutions = GetSubstitutions(availableNodes, scoredAction.Value.RequiredSubstitutions, Graph);

                var scoredHypothesis = new PoolHypothesis(substitutions, scoredAction);
                result.Add(scoredHypothesis);
            }

            result.Sort((a, b) => b.Control.Score.CompareTo(a.Control.Score));
            return result;
        }

        internal static NodesSubstitution GetSubstitutions(IEnumerable<NodeReference> availableNodes, NodesEnumeration originalNodes, ComposedGraph graph)
        {
            var substitutions = new Dictionary<NodeReference, NodeReference>();
            var missingSubstitutionsSet = new HashSet<NodeReference>(originalNodes);
            var availableNodesSet = new HashSet<NodeReference>(availableNodes);

            while (missingSubstitutionsSet.Count > 0)
            {
                NodeReference bestSubstitution = null;
                NodeReference substitutionValue = null;
                double bestDistance = double.MaxValue;

                foreach (var substitution in missingSubstitutionsSet)
                {
                    var nearest = GetNearest(substitution, availableNodesSet, graph);
                    var distance = getDistance(substitution, nearest, graph);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestSubstitution = substitution;
                        substitutionValue = nearest;
                    }
                }

                if (substitutionValue == null)
                    //there are no other substitutions
                    break;

                missingSubstitutionsSet.Remove(bestSubstitution);
                availableNodesSet.Remove(substitutionValue);
                substitutions.Add(bestSubstitution, substitutionValue);
            }
            return new NodesSubstitution(originalNodes, substitutions);
        }

        internal IEnumerable<NodeReference> GetRelatedNodes(ParsedUtterance sentence)
        {
            return GetQuestionEntry(sentence).QuestionNodes;
        }

        internal static NodeReference GetNearest(NodeReference pivot, IEnumerable<NodeReference> nodes, ComposedGraph graph)
        {
            var measuredNodes = new List<Tuple<NodeReference, double>>();
            foreach (var node in nodes)
            {
                var distance = getDistance(pivot, node, graph);
                var measuredNode = Tuple.Create(node, distance);
                measuredNodes.Add(measuredNode);
            }

            measuredNodes.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            if (measuredNodes.Count == 0)
                return pivot;

            return measuredNodes[0].Item1;
        }

        private void fillPool(IEnumerable<NodeReference> context)
        {
            Pool.Insert(context.ToArray());
        }

        private bool updateOldActions(string question, bool isBasedOnContext, NodeReference correctAnswerNode)
        {
            var parsedQuestion = UtteranceParser.Parse(question);
            var bestHypothesis = GetBestHypothesis(parsedQuestion);
            if (bestHypothesis == null)
                return false;

            if (bestHypothesis.Control.Score < 0.9)
                //this is different hypothesis
                return false;

            var questionEntry = GetQuestionEntry(parsedQuestion);

            //update push part of rule
            if (!updatePushPart(questionEntry, bestHypothesis, Pool))
                return false;

            //context filter is updated with push part
            return true;
        }

        private bool updateFilterPart(NodeReference correctAnswerNode, PoolHypothesis bestHypothesis)
        {
            var pool = Pool.Clone();
            runActions(pool, bestHypothesis.ActionBlock, bestHypothesis.Substitutions, false);
            if (pool.ActiveNodes.Contains(correctAnswerNode))
            {
                setFilter(correctAnswerNode, bestHypothesis.ActionBlock, pool);
                return true;
            }

            return false;
        }

        private bool updatePushPart(QuestionEntry questionEntry, PoolHypothesis hypothesis, ContextPool pool)
        {
            pool = pool.Clone();
            if (!questionEntry.IsContextFree)
                //TODO: we are now not able to learn this
                return false;

            pool.ClearAccumulator();
            //we don't need substitute anything - we ran rules with original nodes only
            pool.SetSubstitutions(null);

            var pushActions = new List<PushAction>();
            var insertActions = new List<InsertAction>();

            //traverse all entries and collect all update/insert rules, that will 
            //cover every correct answer
            var equivalentEntries = getPatternEquivalentEntries(questionEntry);
            var correctAnswers = new HashSet<NodeReference>();
            foreach (var entry in equivalentEntries)
            {
                if (!entry.HasAnswer)
                    continue;

                if (!pool.ContainsInAccumulator(entry.CorrectAnswer))
                {
                    var action = createPushAction(entry.Question, entry.CorrectAnswer);
                    if (action == null)
                    {
                        //we cannot derive push rule
                        insertActions.Add(new InsertAction(entry.CorrectAnswer));
                        pool.Insert(entry.CorrectAnswer);
                    }
                    else
                    {
                        //we have got push rule - we will apply it without constraining and filtering
                        action.Run(pool);
                        if (entry.QuestionNodes.Count != hypothesis.Substitutions.OriginalNodes.Count)
                            //TODO we are not able to update this for now
                            return false;

                        action = action.Resubstitution(entry.QuestionNodes, hypothesis.Substitutions.OriginalNodes);
                        pushActions.Add(action);
                    }

                    correctAnswers.Add(entry.CorrectAnswer);
                }
            }

            hypothesis.ActionBlock.UpdatePush(pushActions);
            hypothesis.ActionBlock.UpdateInsert(insertActions);

            foreach (var node in pool.ActiveNodes)
            {
                //TODO detection of inclusion collision in filter
                var isCorrect = correctAnswers.Contains(node);
                hypothesis.ActionBlock.OutputFilter.Advice(node, isCorrect);
            }
            return true;
        }

        private IEnumerable<QuestionEntry> getPatternEquivalentEntries(QuestionEntry entry)
        {
            var pattern = getPatternQuestion(entry);

            return _questionsPatternIndex[pattern];
        }

        private bool createNewActions(string question, bool isBasedOnContext, NodeReference correctAnswerNode)
        {
            ActionBlock actionBlock;
            if (isBasedOnContext)
            {
                actionBlock = extendAdvice(correctAnswerNode);
            }
            else
            {
                actionBlock = pushAdvice(question, correctAnswerNode);
            }

            if (actionBlock == null)
                //we are not able to learn this
                return false;

            var pool = Pool.Clone();
            runActions(pool, actionBlock);
            if (!pool.ActiveNodes.Contains(correctAnswerNode))
                //we are not able to get this advice
                return false;

            setFilter(correctAnswerNode, actionBlock, pool);

            //we want to update mapping of question answering frame
            Triggers.SetMapping(question, actionBlock);
            return true;
        }

        private void setFilter(NodeReference correctAnswerNode, ActionBlock actionBlock, ContextPool pool)
        {
            foreach (var node in pool.ActiveNodes)
            {
                var isInOutput = node.Equals(correctAnswerNode);
                actionBlock.OutputFilter.Advice(node, isInOutput, false);
            }

            actionBlock.OutputFilter.Retrain();
            ConsoleServices.Print(actionBlock.OutputFilter.Root);
        }

        private static void runActions(ContextPool pool, ActionBlock actionBlock, NodesSubstitution substitutions = null, bool useFiltering = true)
        {
            if (substitutions != null)
                pool.SetSubstitutions(substitutions);

            var sortedActions = actionBlock.Actions.OrderByDescending((a) => a.Priority).ToArray();
            var hasPushAction = sortedActions.Any(action => action is PushAction);
            if (hasPushAction)
                //start new topic - but only once! (multiple pushes can appear)
                pool.ClearAccumulator();

            foreach (var action in sortedActions)
            {
                action.Run(pool);
            }

            if (useFiltering && pool.ActiveCount > 1)
            {
                pool.Filter(actionBlock.OutputFilter);
            }
        }

        private static double getDistance(NodeReference node1, NodeReference node2, ComposedGraph graph)
        {
            var paths = graph.GetPaths(node1, node2, MaximumGraphDepth, MaximumGraphWidth).Take(1000);

            double minDistance = double.MaxValue;
            foreach (var path in paths)
            {
                ConsoleServices.Print(path);
                var distance = getDistance(path);

                if (minDistance > distance)
                {
                    minDistance = distance;
                }
            }

            return minDistance;
        }

        private static double getDistance(KnowledgePath path)
        {
            if (path == null)
                return double.PositiveInfinity;

            var distance = 0.0;
            for (var i = 0; i < path.Length; ++i)
            {
                var edge = path.GetEdge(i);
                if (edge.Name == ComposedGraph.IsRelation || edge.Name == "P31")
                    distance += 0.1;
                else
                    distance += 1;
            }

            return distance;
        }

        private IEnumerable<NodeReference> getActualAnswer(PoolHypothesis hypothesis)
        {
            var pool = Pool.Clone();

            pool.SetSubstitutions(hypothesis.Substitutions);
            foreach (var action in hypothesis.ActionBlock.Actions)
            {
                action.Run(pool);
            }

            return pool.ActiveNodes;
        }

        private ActionBlock extendAdvice(NodeReference correctAnswer)
        {
            KnowledgePath shortestPath = null;
            var bestLength = int.MaxValue;
            foreach (var node in Pool.ActiveNodes)
            {
                var path = Graph.GetPaths(node, correctAnswer, 20, 100).FirstOrDefault();
                if (path == null)
                    continue;

                if (path.Length < bestLength)
                {
                    shortestPath = path;
                    bestLength = path.Length;
                }
            }

            IPoolAction action;
            if (shortestPath == null)
                //we doesn't have enough evidence in DB
                action = new InsertAction(correctAnswer);
            else
                action = new ExtendAction(shortestPath);

            return new ActionBlock(Pool.Graph, new[] { action });
        }

        private PushAction createPushAction(string question, NodeReference correctAnswer)
        {
            var relevantUtterances = lastRelevantUtterances(question, correctAnswer);
            var orderedUtterances = (from utterance in relevantUtterances orderby getFowardTargets(utterance).Count select utterance).ToArray();

            if (!orderedUtterances.Any())
                return null;

            var pushPart = orderedUtterances.Last();
            var pushAction = new PushAction(pushPart);
            return pushAction;
        }

        private ActionBlock pushAdvice(string question, NodeReference correctAnswer)
        {
            var pushAction = createPushAction(question, correctAnswer);
            if (pushAction == null)
            {
                //we don't have more evidence - we just have to push given answer
                var block = new ActionBlock(Pool.Graph, new InsertAction(correctAnswer));
                return block;
            }

            var pushedNodes = getFowardTargets(pushAction.SemanticOrigin);

            var relevantUtterances = lastRelevantUtterances(question, correctAnswer);
            var orderedUtterances = (from utterance in relevantUtterances orderby getFowardTargets(utterance).Count select utterance).ToArray();
            var constraints = new List<ConstraintAction>();
            for (var i = 0; i < orderedUtterances.Length - 1; ++i)
            {
                var constraintUtterance = orderedUtterances[i];

                var selectors = getBackwardTargets(constraintUtterance, pushedNodes);
                //var path = getCommonPath(selectors);

                var action = new ConstraintAction(constraintUtterance, constraintUtterance.Paths.First());
                constraints.Add(action);
            }

            var actions = new List<IPoolAction>();
            actions.Add(pushAction);
            actions.AddRange(constraints);

            return new ActionBlock(Pool.Graph, actions);
        }

        private IEnumerable<SemanticPart> lastRelevantUtterances(string question, NodeReference answer)
        {
            var result = new List<SemanticPart>();

            foreach (var word in UtteranceParser.Parse(question).Words)
            {
                var fromNode = Graph.GetNode(word);
                var paths = Graph.GetPaths(fromNode, answer, MaximumGraphDepth, MaximumGraphWidth).Take(1).ToArray();
                if (paths.Length == 0)
                    //there is no evidence
                    continue;

                var part = new SemanticPart(question, paths);
                result.Add(part);
            }

            return result;
        }

        private HashSet<NodeReference> getFowardTargets(SemanticPart part, IEnumerable<NodeReference> startingNodes = null)
        {
            if (startingNodes == null)
                startingNodes = new List<NodeReference>(new[] { part.StartNode });

            var result = new HashSet<NodeReference>();
            foreach (var path in part.Paths)
            {
                var currentLayer = startingNodes;
                for (var i = 0; i < path.Length; ++i)
                {
                    var edge = path.GetEdge(i);
                    var nextLayer = new List<NodeReference>();
                    foreach (var node in currentLayer)
                    {
                        var targets = Graph.Targets(node, edge);
                        nextLayer.AddRange(targets);
                    }

                    currentLayer = nextLayer;
                }

                result.UnionWith(currentLayer);
            }

            return result;
        }

        private HashSet<NodeReference> getBackwardTargets(SemanticPart part, IEnumerable<NodeReference> endingNodes = null)
        {
            if (endingNodes == null)
                endingNodes = new List<NodeReference>(new[] { part.StartNode });

            var result = new HashSet<NodeReference>();
            foreach (var path in part.Paths)
            {
                var currentLayer = endingNodes;
                for (var i = path.Length - 1; i >= 0; --i)
                {
                    var edge = path.GetEdge(i).Inverse();
                    var nextLayer = new List<NodeReference>();
                    foreach (var node in currentLayer)
                    {
                        var targets = Graph.Targets(node, edge);
                        nextLayer.AddRange(targets);
                    }

                    currentLayer = nextLayer;
                }

                result.UnionWith(currentLayer);
            }

            return result;
        }

        private NodeReference getNode(string word)
        {
            return Graph.GetNode(word);
        }

        private HashSet<Edge> getCommonEdges(IEnumerable<NodeReference> nodes)
        {
            var commonEdges = new HashSet<Edge>();
            commonEdges.UnionWith(getEdges(nodes.First()));
            foreach (var node in nodes)
            {
                var nodeEdges = getEdges(node);
                commonEdges.IntersectWith(nodeEdges);
            }
            return commonEdges;
        }

        private Edge[] getEdges(NodeReference node)
        {
            var nodeTargets = Graph.GetNeighbours(node, MaximumGraphWidth);
            var nodeEdges = (from nodeTarget in nodeTargets select nodeTarget.Item1).ToArray();
            return nodeEdges;
        }

        private string getPatternQuestion(QuestionEntry question)
        {
            var builder = new StringBuilder();
            foreach (var word in question.ParsedQuestion.Words)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                if (Graph.HasEvidence(word))
                    builder.Append("#");
                else
                    builder.Append(word);
            }

            return builder.ToString();
        }
    }
}
