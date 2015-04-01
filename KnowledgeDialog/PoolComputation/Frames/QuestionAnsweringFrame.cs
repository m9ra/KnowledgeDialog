using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.ModifiableResponses;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.Frames
{
    class QuestionAnsweringFrame : ConversationFrameBase
    {
        public static readonly int MaximumUserReport = 2;

        public static readonly int MaximumWidth = 100;

        protected ContextPool Pool { get { return _context.Pool; } }

        private readonly string NoResults = "I have no matching data";

        private readonly DialogContext _context;

        private string _lastQuestion;

        public QuestionAnsweringFrame(ConversationContext conversationContext, DialogContext context)
            : base(conversationContext)
        {
            _context = context;
            EnsureInitialized<UtteranceMapping<ActionBlock>>(() => new UtteranceMapping<ActionBlock>(_context.Graph));
        }

        protected override ModifiableResponse FrameInitialization()
        {
            return DefaultHandler();
        }

        protected override ModifiableResponse DefaultHandler()
        {
            var utterance = CurrentInput;
            if (utterance.StartsWith("it is", StringComparison.InvariantCultureIgnoreCase))
            {
                return Response(new QueryAdviceFrame(ConversationContext, _lastQuestion, _context));
            }

            var bestHypothesis = getHypothesis(utterance).FirstOrDefault();
            if (bestHypothesis == null || bestHypothesis.Item2 < 0.3)
            {
                return Response(new QueryAdviceFrame(ConversationContext, utterance, _context));
            }


            _lastQuestion = utterance;

            Pool.SetSubstitutions(bestHypothesis.Item1.Substitutions);
            foreach (var action in bestHypothesis.Item1.ActionBlock.Actions)
            {
                action.Run(Pool);
            }
            Pool.Filter(bestHypothesis.Item1.ActionBlock.OutputFilter);


            if (Pool.ActiveCount <= MaximumUserReport)
            {
                if (!Pool.HasActive)
                {
                    return Response(NoResults);
                }
                else
                {
                    return Response("It is", Pool.ActiveNodes);
                }
            }
            else
            {
                throw new NotImplementedException("Find criterion");
            }
        }

        private IEnumerable<Tuple<PoolHypothesis, double>> getHypothesis(string utterance)
        {
            var scoredActions = Get<UtteranceMapping<ActionBlock>>().ScoredMap(utterance);

            var availableNodes = getRelatedNodes(utterance);


            var result = new List<Tuple<PoolHypothesis, double>>();
            foreach (var scoredAction in scoredActions)
            {
                var substitutions = new Dictionary<NodeReference, NodeReference>();
                foreach (var node in scoredAction.Item1.RequiredSubstitutions)
                {
                    var nearestNode = getNearest(node, availableNodes);
                    substitutions.Add(node, nearestNode);
                }

                var scoredHypothesis = Tuple.Create(new PoolHypothesis(substitutions, scoredAction.Item1), scoredAction.Item2);
                result.Add(scoredHypothesis);
            }

            return result;
        }

        private IEnumerable<NodeReference> getRelatedNodes(string utterance)
        {
            foreach (var node in utterance.Split(' '))
            {
                if (_context.Graph.HasEvidence(node))
                    yield return _context.Graph.GetNode(node);
            }
        }

        private NodeReference getNearest(NodeReference pivot, IEnumerable<NodeReference> nodes)
        {
            var measuredNodes = new List<Tuple<NodeReference,double>>();
            foreach (var node in nodes) {
                var path= _context.Graph.GetPaths(pivot, node, MaximumWidth, MaximumWidth).FirstOrDefault();
                var distance = getDistance(path);

                var measuredNode = Tuple.Create(node, distance);
                measuredNodes.Add(measuredNode);
            }

            measuredNodes.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            if (measuredNodes.Count == 0)
                return pivot;

            return measuredNodes[0].Item1;
        }

        private double getDistance(KnowledgePath path)
        {
            if (path == null)
                return double.PositiveInfinity;

            var distance=0.0;
            for (var i = 0; i < path.Length; ++i)
            {
                if (path.Edge(i) == ComposedGraph.IsRelation)
                    distance += 0.1;
                else
                    distance += 1;
            }

            return distance;
        }
    }
}
