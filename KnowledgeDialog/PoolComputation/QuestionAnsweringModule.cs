using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog;

using KnowledgeDialog.PoolComputation.PoolActions;

namespace KnowledgeDialog.PoolComputation
{
    class QuestionAnsweringModule
    {
        private Dictionary<string, IEnumerable<NodeReference>> _explicitAnswers = new Dictionary<string, IEnumerable<NodeReference>>();

        internal readonly ContextPool Pool;

        internal ComposedGraph Graph { get { return Pool.Graph; } }

        internal readonly UtteranceMapping<ActionBlock> Triggers;

        internal readonly int MaximumGraphDepth = 3;

        internal readonly int MaximumGraphWidth = 1000;

        internal QuestionAnsweringModule(ComposedGraph graph)
        {
            Pool = new ContextPool(graph);
            Triggers = new UtteranceMapping<ActionBlock>(graph);
        }

        public IEnumerable<NodeReference> GetAnswer(string question)
        {
            IEnumerable<NodeReference> explicitAnswer;
            if (_explicitAnswers.TryGetValue(question, out explicitAnswer))
            {
                return explicitAnswer;
            }

            var bestHypothesis = GetBestHypothesis(question);
            if (bestHypothesis == null)
                return new NodeReference[0];

            var pool = Pool.Clone();

            pool.SetSubstitutions(bestHypothesis.Item1.Substitutions);
            foreach (var action in bestHypothesis.Item1.Actions)
            {
                action.Run(pool);
            }

            return pool.ActiveNodes;
        }

        public void AdviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode)
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

            //we want to update mapping of question answering frame
            Triggers.SetMapping(question, actionBlock);
            _explicitAnswers[question] = new[] { correctAnswerNode };
        }


        public void SuggestAnswer(string question, NodeReference suggestedAnswer)
        {
            _explicitAnswers[question] = new[] { suggestedAnswer };

            var hypotheses = GetControlledHypotheses(question).ToArray();

            foreach (var hypothesis in hypotheses)
            {
                var actualAnswer = getActualAnswer(hypothesis.Item1);
                var isCorrect = actualAnswer.Contains(suggestedAnswer);

                hypothesis.Item2.Suggest(isCorrect);
            }

            if (suggestedAnswer != null)
            {
                var isBasedOnContext = hypotheses.Length > 0 && hypotheses[0].Item1.Actions.Any(a => a is ExtendAction);
                AdviceAnswer(question, isBasedOnContext, suggestedAnswer);
            }
        }

        public void Negate(string question)
        {
            throw new NotImplementedException();
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
            var availableNodes = GetRelatedNodes(utterance).ToArray();

            var result = new List<Tuple<PoolHypothesis, MappingControl>>();
            foreach (var scoredAction in scoredActions)
            {
                var substitutions = new Dictionary<NodeReference, NodeReference>();
                foreach (var node in scoredAction.Item1.RequiredSubstitutions)
                {
                    var nearestNode = GetNearest(node, availableNodes);
                    substitutions.Add(node, nearestNode);
                }

                var scoredHypothesis = Tuple.Create(new PoolHypothesis(substitutions, scoredAction.Item1.Actions), scoredAction.Item2);
                result.Add(scoredHypothesis);
            }

            return result;
        }

        internal IEnumerable<NodeReference> GetRelatedNodes(string utterance)
        {
            foreach (var node in utterance.Split(' '))
            {
                if (Graph.HasEvidence(node))
                    yield return Graph.GetNode(node);
            }
        }

        internal NodeReference GetNearest(NodeReference pivot, IEnumerable<NodeReference> nodes)
        {
            var measuredNodes = new List<Tuple<NodeReference, double>>();
            foreach (var node in nodes)
            {
                var paths = Graph.GetPaths(pivot, node, MaximumGraphDepth, MaximumGraphWidth).Take(1000);

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
                var measuredNode = Tuple.Create(node, minDistance);
                measuredNodes.Add(measuredNode);
            }

            measuredNodes.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            if (measuredNodes.Count == 0)
                return pivot;

            return measuredNodes[0].Item1;
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
            foreach (var action in hypothesis.Actions)
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

            if (shortestPath == null)
                throw new NotImplementedException("There is no extending path");

            var poolAction = new ExtendAction(shortestPath);
            return new ActionBlock(new[] { poolAction });
        }

        private ActionBlock pushAdvice(string question, NodeReference correctAnswer)
        {
            var relevantUtterances = lastRelevantUtterances(question, correctAnswer);
            var orderedUtterances = (from utterance in relevantUtterances orderby getFowardTargets(utterance).Count select utterance).ToArray();

            if (!orderedUtterances.Any())
            {
                //we don't have more evidence - we just have to push given answer
                var block = new ActionBlock(new InsertAction(correctAnswer));
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

            return new ActionBlock(actions);
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

        private KnowledgePath getCommonPath(IEnumerable<NodeReference> nodesEnumeration)
        {
            var nodes = nodesEnumeration.ToArray();
            if (nodes.Length < 2)
                throw new NotImplementedException();

            //TODO this is simple implementation - should be improved
            foreach (var path in Graph.GetPaths(nodes[0], nodes[1], MaximumGraphDepth, MaximumGraphWidth))
            {
                //Test if path is a pallindrome
                if (path.Length % 2 == 1)
                    //path has to be even to be pallindrome
                    continue;

                var isPallindrome = true;
                for (var i = 0; i < path.Length / 2; ++i)
                {
                    var j = path.Length - i - 1;
                    var edge1 = path.Edge(i);
                    var edge2 = path.Edge(j);
                    var isOut1 = path.IsOutcomming(i);
                    var isOut2 = path.IsOutcomming(j);

                    if (edge1 != edge2 || isOut1 == isOut2)
                    {
                        isPallindrome = false;
                        break;
                    }
                }

                if (isPallindrome)
                    return path.TakeEnding(path.Length / 2);
            }

            return null;
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
