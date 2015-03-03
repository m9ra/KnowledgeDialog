using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.PoolComputation.PoolActions;

namespace KnowledgeDialog.PoolComputation.Frames
{
    class QueryAdviceFrame : ConversationFrameBase
    {
        private readonly DialogContext _context;

        private readonly string _unknownQuestion;

        private bool _expectCorrectAnswer;

        private bool _isExtendUncertain;

        private bool _isExtend;

        private bool _hasAskedForAnswer;

        private bool _hasAskedForExtendUncertainity;

        private NodeReference _correctAnswer;

        internal QueryAdviceFrame(ConversationContext conversationContext, string unknownQuestion, DialogContext context)
            : base(conversationContext)
        {
            _context = context;
            _unknownQuestion = unknownQuestion;
        }

        protected override ModifiableResponse FrameInitialization()
        {
            _isExtendUncertain = _context.Pool.ActiveCount > 0;
            _expectCorrectAnswer = _unknownQuestion != CurrentInput;

            return DefaultHandler();
        }

        protected override ModifiableResponse DefaultHandler()
        {
            if (_expectCorrectAnswer)
            {
                _correctAnswer = parseAnswer(CurrentInput);
            }

            if (_isExtendUncertain)
            {
                _isExtendUncertain = false;
                _hasAskedForExtendUncertainity = true;
                return YesNoQuestion("I cannot fully understand your question. Are you asking for something connected with your previous question?",
                    () => _isExtend = true,
                    () => _isExtend = false
                    );
                throw new NotImplementedException("Ask for extend and set _isExtend accordingly");
            }

            var hasCorrectAnswer = _correctAnswer != null;
            if (!hasCorrectAnswer && !_hasAskedForAnswer)
            {
                _hasAskedForAnswer = true;
                _expectCorrectAnswer = true;

                if (_hasAskedForExtendUncertainity)
                    return Response("I know lot of information connected with your previous question. Can you please advice me the correct answer?");
                else
                    return Response("Please, can you give me correct answer for your question?");

            }

            if (_hasAskedForAnswer && CurrentInput.Contains(" no "))
            {
                throw new NotImplementedException("user refuse to tell correct answer");
            }

            ActionBlock actionBlock;
            if (_isExtend)
            {
                actionBlock = extendAdvice();
            }
            else
            {
                actionBlock = pushAdvice();
            }

            _context.Pool.Insert(_correctAnswer);


            //we want to update mapping of question answering frame
            Get<QuestionAnsweringFrame, UtteranceMapping<ActionBlock>>().SetMapping(_unknownQuestion, actionBlock);

            IsComplete = true;
            return Response("Thank you");
        }

        private ActionBlock extendAdvice()
        {
            KnowledgePath shortestPath = null;
            var bestLength = int.MaxValue;
            foreach (var node in _context.Pool.ActiveNodes)
            {
                var path = _context.Graph.GetPaths(node, _correctAnswer, 20, 100).FirstOrDefault();
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

        private ActionBlock pushAdvice()
        {
            var relevantUtterances = lastRelevantUtterances(_correctAnswer);
            var orderedUtterances = (from utterance in relevantUtterances orderby getFowardTargets(utterance).Count select utterance).ToArray();

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
                        var targets = _context.Graph.Targets(node, edge, isOutcomming);
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
                        var targets = _context.Graph.Targets(node, edge, isOutcomming);
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
            return _context.Graph.GetNode(word);
        }

        private KnowledgePath getCommonPath(IEnumerable<NodeReference> nodesEnumeration)
        {
            var nodes = nodesEnumeration.ToArray();
            if (nodes.Length < 2)
                throw new NotImplementedException();

            //TODO this is simple implementation - should be improved
            foreach (var path in _context.Graph.GetPaths(nodes[0], nodes[1], 20, QuestionAnsweringFrame.MaximumWidth))
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
            var nodeTargets = _context.Graph.GetNeighbours(node, QuestionAnsweringFrame.MaximumWidth);
            var nodeEdges = (from nodeTarget in nodeTargets select Tuple.Create(nodeTarget.Item1, nodeTarget.Item2)).ToArray();
            return nodeEdges;
        }

        private IEnumerable<SemanticPart> lastRelevantUtterances(NodeReference answer)
        {
            var result = new List<SemanticPart>();

            var sentence = _unknownQuestion;
            foreach (var word in getWords(sentence))
            {
                var fromNode = _context.Graph.GetNode(word);
                var paths = _context.Graph.GetPaths(fromNode, answer, 20, QuestionAnsweringFrame.MaximumWidth).Take(1).ToArray();
                if (paths.Length == 0)
                    //there is no evidence
                    continue;

                var part = new SemanticPart(sentence, paths);
                result.Add(part);
            }

            return result;
        }

        private string parseQuestion(string utterance)
        {
            return utterance;
        }

        private NodeReference parseAnswer(string utterance)
        {
            var prefix = "it is";
            return getNode(utterance.Substring(prefix.Length).Trim());
        }

        private IEnumerable<string> getWords(string sentence)
        {
            return sentence.Split(' ');
        }
    }
}
