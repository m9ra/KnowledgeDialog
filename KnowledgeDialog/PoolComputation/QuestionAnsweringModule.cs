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
    public class QuestionAnsweringModule
    {
        private readonly object _L_input = new object();

        internal readonly ContextPool Pool;

        internal ComposedGraph Graph { get { return Pool.Graph; } }

        internal readonly UtteranceMapping<ActionBlock> Triggers;

        internal static readonly int MaximumGraphDepth = 3;

        internal static readonly int MaximumGraphWidth = 1000;

        private readonly CallSerializer _adviceAnswer;

        private readonly CallSerializer _repairAnswer;

        private readonly CallSerializer _setEquivalencies;

        private readonly CallSerializer _negate;

        internal readonly CallStorage Storage;

        public QuestionAnsweringModule(ComposedGraph graph, CallStorage storage)
        {
            Storage = storage;
            Pool = new ContextPool(graph);
            Triggers = new UtteranceMapping<ActionBlock>(graph);

            _adviceAnswer = storage.RegisterCall("AdviceAnswer", c =>
            {
                _AdviceAnswer(c.String("question"), c.Bool("isBasedOnContext"), c.Node("correctAnswerNode", Graph), c.Nodes("context", Graph));
            });

            _repairAnswer = storage.RegisterCall("RepairAnswer", c =>
            {
                _RepairAnswer(c.String("question"), c.Node("suggestedAnswer", Graph), c.Nodes("context", Graph));
            });

            _setEquivalencies = storage.RegisterCall("SetEquivalence", c =>
            {
                SetEquivalence(c.String("patternQuestion"), c.String("queriedQuestion"), c.Bool("isEquivalent"));
            });

            _negate = storage.RegisterCall("Negate", c =>
            {
                Negate(c.String("question"));
            });
        }

        #region Input methods

        public bool AdviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode)
        {
            lock (_L_input)
            {
                return _AdviceAnswer(question, isBasedOnContext, correctAnswerNode, Pool.ActiveNodes);
            }
        }

        private bool _AdviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            _adviceAnswer.ReportParameter("question", question);
            _adviceAnswer.ReportParameter("isBasedOnContext", isBasedOnContext);
            _adviceAnswer.ReportParameter("correctAnswerNode", correctAnswerNode);
            _adviceAnswer.ReportParameter("context", context);
            _adviceAnswer.SaveReport();

            fillPool(context);

            return
                updateOldActions(question, isBasedOnContext, correctAnswerNode) ||
                createNewActions(question, isBasedOnContext, correctAnswerNode);
        }

        public void RepairAnswer(string question, NodeReference suggestedAnswer)
        {
            lock (_L_input)
            {
                _RepairAnswer(question, suggestedAnswer, Pool.ActiveNodes);
            }
        }

        public void _RepairAnswer(string question, NodeReference suggestedAnswer, IEnumerable<NodeReference> context)
        {
            _repairAnswer.ReportParameter("question", question);
            _repairAnswer.ReportParameter("suggestedAnswer", suggestedAnswer);
            _repairAnswer.ReportParameter("context", context);
            _repairAnswer.SaveReport();

            fillPool(context);
            var hypotheses = GetControlledHypotheses(question).ToArray();

            foreach (var hypothesis in hypotheses)
            {
                var actualAnswer = getActualAnswer(hypothesis.Item1);
                var isCorrect = actualAnswer.Contains(suggestedAnswer);

                hypothesis.Item2.Suggest(isCorrect);
            }

            if (suggestedAnswer != null)
            {
                var isBasedOnContext = hypotheses.Length > 0 && hypotheses[0].Item1.ActionBlock.Actions.Any(a => a is ExtendAction);
                AdviceAnswer(question, isBasedOnContext, suggestedAnswer);
            }
        }

        public void SetEquivalence(string patternQuestion, string queriedQuestion, bool isEquivalent)
        {
            lock (_L_input)
            {
                _setEquivalencies.ReportParameter("patternQuestion", patternQuestion);
                _setEquivalencies.ReportParameter("queriedQuestion", queriedQuestion);
                _setEquivalencies.ReportParameter("isEquivalent", isEquivalent);
                _setEquivalencies.SaveReport();

                if (isEquivalent)
                {
                    var bestHyp = Triggers.BestMap(patternQuestion);
                    Triggers.SetMapping(queriedQuestion, bestHyp);
                }
                else
                {
                    Triggers.DisableEquivalence(patternQuestion, queriedQuestion);
                }
            }
        }

        public void Negate(string question)
        {
            lock (_L_input)
            {
                _negate.ReportParameter("question", question);
                _negate.SaveReport();

                var bestHypothesis = GetHypotheses(question).FirstOrDefault();
                if (bestHypothesis == null)
                    //we cannot learn anything
                    return;

                var currentAnswer = getActualAnswer(bestHypothesis.Item1);
                foreach (var answer in currentAnswer)
                {
                    bestHypothesis.Item1.ActionBlock.OutputFilter.Advice(answer, false);
                }
            }
        }

        #endregion

        public IEnumerable<NodeReference> GetAnswer(string question)
        {
            var bestHypothesis = GetBestHypothesis(question);
            if (bestHypothesis == null)
                return new NodeReference[0];

            var pool = Pool.Clone();

            var substitutions = bestHypothesis.Item1.Substitutions;
            var block = bestHypothesis.Item1.ActionBlock;
            runActions(pool, block, substitutions);

            return pool.ActiveNodes;
        }

        internal Tuple<PoolHypothesis, double> GetBestHypothesis(string question)
        {
            return GetHypotheses(question).FirstOrDefault();
        }

        internal IEnumerable<Tuple<PoolHypothesis, double>> GetHypotheses(string utterance)
        {
            return from h in GetControlledHypotheses(utterance) select Tuple.Create(h.Item1, h.Item2.Score);
        }

        internal IEnumerable<Tuple<PoolHypothesis, MappingControl>> GetControlledHypotheses(string utterance)
        {
            var scoredActions = Triggers.ControlledMap(utterance);
            var availableNodes = GetRelatedNodes(utterance, Graph).ToArray();

            var result = new List<Tuple<PoolHypothesis, MappingControl>>();
            foreach (var scoredAction in scoredActions)
            {
                var substitutions = GetSubstitutions(availableNodes, scoredAction.Item1.RequiredSubstitutions, Graph);

                var scoredHypothesis = Tuple.Create(new PoolHypothesis(substitutions, scoredAction.Item1), scoredAction.Item2);
                result.Add(scoredHypothesis);
            }

            return result;
        }

        internal static Dictionary<NodeReference, NodeReference> GetSubstitutions(IEnumerable<NodeReference> availableNodes,IEnumerable<NodeReference> requiredSubstitutions, ComposedGraph graph)
        {
            var substitutions = new Dictionary<NodeReference, NodeReference>();
            var missingSubstitutionsSet = new HashSet<NodeReference>();
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
            return substitutions;
        }

        internal static IEnumerable<NodeReference> GetRelatedNodes(string utterance, ComposedGraph graph)
        {
            foreach (var node in utterance.Split(' '))
            {
                if (graph.HasEvidence(node))
                    yield return graph.GetNode(node);
            }
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
            var bestHypothesis = GetHypotheses(question).FirstOrDefault();
            if (bestHypothesis == null)
                return false;

            if (bestHypothesis.Item2 < 0.9)
                //this is different hypothesis
                return false;

            var pool = Pool.Clone();
            runActions(pool, bestHypothesis.Item1.ActionBlock, bestHypothesis.Item1.Substitutions, false);
            if (pool.ActiveNodes.Contains(correctAnswerNode))
            {
                setFilter(correctAnswerNode, bestHypothesis.Item1.ActionBlock, pool);
                return true;
            }

            return false;
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
                actionBlock.OutputFilter.Advice(node, isInOutput);
            }

            ConsoleServices.Print(actionBlock.OutputFilter.Root);
        }

        private static void runActions(ContextPool pool, ActionBlock actionBlock, IEnumerable<KeyValuePair<NodeReference, NodeReference>> substitutions = null, bool useFiltering = true)
        {
            if (substitutions != null)
                pool.SetSubstitutions(substitutions);

            foreach (var action in actionBlock.Actions)
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
                var edge = path.Edge(i);
                if (edge == ComposedGraph.IsRelation || edge == "P31")
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

        private ActionBlock pushAdvice(string question, NodeReference correctAnswer)
        {
            var relevantUtterances = lastRelevantUtterances(question, correctAnswer);
            var orderedUtterances = (from utterance in relevantUtterances orderby getFowardTargets(utterance).Count select utterance).ToArray();

            if (!orderedUtterances.Any())
            {
                //we don't have more evidence - we just have to push given answer
                var block = new ActionBlock(Pool.Graph, new InsertAction(correctAnswer));
                return block;
            }

            var pushPart = orderedUtterances.Last();
            var pushAction = new PushAction(pushPart);
            var pushedNodes = getFowardTargets(pushPart);

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

            foreach (var word in SentenceParser.Parse(question).Words)
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
                    var edge = path.Edge(i);
                    var isOutcomming = path.IsOutcomming(i);
                    var nextLayer = new List<NodeReference>();
                    foreach (var node in currentLayer)
                    {
                        var targets = Graph.Targets(node, edge, isOutcomming);
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
                    var edge = path.Edge(i);
                    var isOutcomming = !path.IsOutcomming(i);
                    var nextLayer = new List<NodeReference>();
                    foreach (var node in currentLayer)
                    {
                        var targets = Graph.Targets(node, edge, isOutcomming);
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

        private HashSet<Tuple<string, bool>> getCommonEdges(IEnumerable<NodeReference> nodes)
        {
            var commonEdges = new HashSet<Tuple<string, bool>>();
            commonEdges.UnionWith(getEdges(nodes.First()));
            foreach (var node in nodes)
            {
                var nodeEdges = getEdges(node);
                commonEdges.IntersectWith(nodeEdges);
            }
            return commonEdges;
        }

        private Tuple<string, bool>[] getEdges(NodeReference node)
        {
            var nodeTargets = Graph.GetNeighbours(node, MaximumGraphWidth);
            var nodeEdges = (from nodeTarget in nodeTargets select Tuple.Create(nodeTarget.Item1, nodeTarget.Item2)).ToArray();
            return nodeEdges;
        }
    }
}
