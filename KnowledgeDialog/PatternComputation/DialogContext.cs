using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.PatternComputation.Actions;


namespace KnowledgeDialog.PatternComputation
{
    class DialogContext
    {
        private readonly int _widthLimit = 100;
        private readonly int _lengthLimit = 50;
        private readonly int _pathCountLimit = 15;

        public static readonly double DontKnowThreshold = 0.10;

        /// <summary>
        /// Turns that has been made during the dialog.
        /// </summary>
        private readonly List<DialogTurn> _turns = new List<DialogTurn>();

        /// <summary>
        /// Currently active patterns.
        /// </summary>
        private readonly List<WeightedPattern> _activePatterns = new List<WeightedPattern>();

        /// <summary>
        /// Groups that are required for evaluation.
        /// </summary>
        private readonly HashSet<KnowledgeGroup> _activeGroups = new HashSet<KnowledgeGroup>();

        /// <summary>
        /// Patterns that are currently active in dialog context.
        /// </summary>
        internal IEnumerable<WeightedPattern> ActivePatterns { get { return _activePatterns; } }

        /// <summary>
        /// Groups that are required for active patterns.
        /// </summary>
        internal IEnumerable<KnowledgeGroup> ActiveGroups { get { return _activeGroups; } }

        /// <summary>
        /// Current turn of dialog.
        /// </summary>
        internal DialogTurn CurrentTurn
        {
            get
            {
                if (_turns.Count == 0)
                    return null;

                return _turns[_turns.Count - 1];
            }
        }

        /// <summary>
        /// Create new turn in the context of dialog.
        /// </summary>
        internal void NewTurn(ComposedGraph graph)
        {
            if (CurrentTurn != null)
                CurrentTurn.Close();

            _turns.Add(new DialogTurn(graph));
        }

        internal WeightedPattern FindClosestPattern(ResponseBase answer)
        {
            var evaluationContext = EvaluateGroups(_activeGroups, CurrentTurn.Graph);
            return null;
            foreach (var pattern in _activePatterns)
            {
                var response = pattern.Action.Execute(evaluationContext, pattern.ContextGroup);
                if (response.Equals(answer))
                    throw new NotImplementedException("we have pattern with same answer");
            }

            return null;
        }

        /// <summary>
        /// Create pattern that will generate given answer.
        /// </summary>
        /// <param name="answer">Answer that should be provided by pattern.</param>
        /// <returns>Created pattern.</returns>
        internal IEnumerable<WeightedPattern> CreatePatterns(ResponseBase answer, IEnumerable<NodeReference> inputContextNodes)
        {
            var result = new List<WeightedPattern>();

            var actions = findPossibleActions(answer, CurrentTurn.Graph, inputContextNodes);
            foreach (var action in actions)
            {
                var knowledgeGroup = getActiveGroup(action.ContextNodes, inputContextNodes, CurrentTurn.Graph);
                var pattern = new WeightedPattern(knowledgeGroup, action);

                result.Add(pattern);
            }

            return result;
        }


        internal ResponseBase Negate()
        {
            var evaluation = new EvaluationContext(_activeGroups, CurrentTurn.Graph);
            var scoredPatterns = evaluation.GetScoredPatterns(_activePatterns);
            var negatedAnswer = getBestResponse(evaluation);

            foreach (var scoredPattern in scoredPatterns)
            {
                var patternResponse = scoredPattern.Item1.Action.Execute(evaluation, scoredPattern.Item1.ContextGroup);
                if (patternResponse.Equals(negatedAnswer))
                {
                    //discard most active features of pattern, until it is below treshold
                    scoredPattern.Item1.ChangeWeights(scoredPattern.Item2 - DontKnowThreshold, evaluation);
                }
            }

            return new SimpleResponse("Ok, my mistake.");
        }

        /// <summary>
        /// Add given pattern into contexts evaluation process.
        /// </summary>
        /// <param name="newPattern">Activated pattern.</param>
        internal void ActivatePattern(WeightedPattern newPattern)
        {
            _activePatterns.Add(newPattern);

            //register groups which evaluation is required
            foreach (var feature in newPattern.Features)
            {
                _activeGroups.Add(feature.ContainingGroup);
            }
        }

        internal ResponseBase CreateActualResponse()
        {
            var evaluationContext = EvaluateGroups(_activeGroups, CurrentTurn.Graph);
            if (evaluationContext.IsEmpty)
                return new SimpleResponse("I have no idea.");

            return getBestResponse(evaluationContext);
        }

        /// <summary>
        /// Evaluate given groups against given context.
        /// </summary>
        /// <param name="groups">Groups to be evaluated.</param>
        /// <param name="context">Context where groups will be evaluated.</param>
        /// <returns>Evaluated groups.</returns>
        internal EvaluationContext EvaluateGroups(IEnumerable<KnowledgeGroup> groups, ComposedGraph context)
        {
            return new EvaluationContext(groups, context);
        }

        internal IEnumerable<Tuple<ResponseBase, double>> GetScoredResponses(IEnumerable<WeightedPattern> patterns)
        {
            var evaluationContext = new EvaluationContext(_activeGroups, CurrentTurn.Graph);

            var scoredPatterns = evaluationContext.GetScoredPatterns(patterns);
            foreach (var scoredPattern in scoredPatterns)
            {
                var response = scoredPattern.Item1.Action.Execute(evaluationContext, scoredPattern.Item1.ContextGroup);
                yield return Tuple.Create(response, scoredPattern.Item2);
            }
        }

        internal ResponseBase AdviceResponse()
        {
            return new SimpleResponse("Thank you.");
        }

        internal object FindCurrentBestFeatures(ResponseBase answerResponse)
        {
            throw new NotImplementedException();
        }

        internal void Evaluate(KnowledgeGroup group, ComposedGraph graph)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<object> FindFalsePositives(object features, ResponseBase answerResponse)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate possible actions, that will generate given answer in given graph.
        /// </summary>
        /// <param name="answer">Desired answer.</param>
        /// <param name="contextGraph">Context graph for generated actions.</param>
        /// <returns>Actions that will generate given answer.</returns>
        private IEnumerable<ActionBase> findPossibleActions(ResponseBase answer, ComposedGraph contextGraph, IEnumerable<NodeReference> inputContext)
        {
            if (answer is SimpleResponse)
            {
                yield return new InformAction(answer as SimpleResponse, contextGraph);
            }else if (answer is CountResponse)
            {
                var countResponse = answer as CountResponse;
                foreach (var input in inputContext)
                {
                    var edges = contextGraph.GetNeighbours(input, _widthLimit).ToArray();
                    var edgeCounts = new Dictionary<Tuple<string, bool>, int>();
                    foreach (var outEdge in edges)
                    {
                        var edge = Tuple.Create(outEdge.Item1, outEdge.Item2);
                        int count;
                        edgeCounts.TryGetValue(edge, out count);
                        edgeCounts[edge] = count + 1;
                    }

                    var matchingEdges = edgeCounts.Where((c) => c.Value == countResponse.Count).Select(p => p.Key);
                    foreach (var candidateEdge in matchingEdges)
                    {
                        yield return new CountAction(input, candidateEdge);
                    }
                }
            }
            else
            {
                throw new NotImplementedException("Another actions are not implemented yet");
            }
        }

        /// <summary>
        /// Get knowledge group as first estimation of context nodes recognizer from active output.
        /// </summary>
        /// <param name="contextNodes">Context nodes that should be recognized from output.</param>
        /// <returns></returns>
        private KnowledgeGroup getActiveGroup(IEnumerable<NodeReference> contextNodes, IEnumerable<NodeReference> inputContextNodes, ComposedGraph contextGraph)
        {
            var groupPaths = new List<KnowledgePath>();
            var activeNode = contextGraph.GetNode(ComposedGraph.Active);
            foreach (var contextNode in contextNodes)
            {
                var nodePaths = contextGraph.GetPaths(activeNode, contextNode, _lengthLimit, _widthLimit).Take(_pathCountLimit).ToArray();
                groupPaths.AddRange(nodePaths);
            }

            foreach (var contextNode in inputContextNodes)
            {
                var nodePaths = contextGraph.GetPaths(activeNode, contextNode, _lengthLimit, _widthLimit).Take(_pathCountLimit).ToArray();
                groupPaths.AddRange(nodePaths);
            }

            return new KnowledgeGroup(groupPaths);
        }

        private ResponseBase getBestResponse(EvaluationContext evaluationContext)
        {
            var bestPattern = getBestPattern(evaluationContext);
            return bestPattern.Action.Execute(evaluationContext, bestPattern.ContextGroup);
        }

        private WeightedPattern getBestPattern(EvaluationContext evaluationContext)
        {
            var patterns = evaluationContext.GetSortedPatterns(_activePatterns);
            var bestPattern = patterns.First();
            return bestPattern;
        }
    }
}
