using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.PoolComputation.PoolActions;

using KnowledgeDialog.PoolComputation.StateDialog;
using KnowledgeDialog.PoolComputation.StateDialog.States;

namespace KnowledgeDialog.PoolComputation
{
    public class StateDialogManager : IDialogManager
    {
        private readonly StateContext _context;

        private readonly Dictionary<Type, StateGraphBuilder> _buildersIndex = new Dictionary<Type, StateGraphBuilder>();

        private readonly List<StateGraphBuilder> _createdBuilders = new List<StateGraphBuilder>();

        private readonly StateGraphBuilder _equivalenceQuestionState;

        private StateGraphBuilder _currentState = null;

        private readonly StateGraphBuilder QuestionRouting;

        public StateDialogManager(string serializationOutput, params GraphLayerBase[] layers)
            : this(new StateContext(new ComposedGraph(layers), serializationOutput))
        {
        }

        public StateDialogManager(StateContext context)
        {
            _context = context;
            _context.CallStorage.ReadStorage();

            S<Welcome>()
                .Default(S<HowCanIHelp>());

            QuestionRouting = EmptyState(true);
            S<HowCanIHelp>()
                .Default(QuestionRouting);

            S<ApologizeState>()
                .Default(QuestionRouting);

            S<RequestContext>()
                .YesNoEdge(S<RequestContext>(), AcceptAdvice.IsBasedOnContextProperty)
                .Default(S<ApologizeState>());

            S<RequestAnswer>()
                .IsEdge(S<AcceptAdvice>(), AcceptAdvice.CorrectAnswerProperty)                
                .Default(S<ApologizeState>());

            S<AcceptAdvice>()
                //when info is missing, we have to go back to advice routing
                .Edge(AcceptAdvice.MissingInfoEdge, S<RequestAnswer>())
                //when advice handling is completed go to QuestionRouting
                .Default(QuestionRouting);

            QuestionRouting
                //we know requested question
                .HasMatch(_context.QuestionAnsweringModule.Triggers, S<QuestionAnswering>(), RequestAnswer.QuestionProperty)

                .Edge(EquivalenceQuestion.EquivalenceEdge, S<EquivalenceQuestion>())

                .IsEdge(S<AcceptAdvice>(), AcceptAdvice.CorrectAnswerProperty)             

                //question is not recognized as advice
                .Default(S<RequestAnswer>(), RequestAnswer.QuestionProperty);

            S<QuestionAnswering>()
                .TakeEdgesFrom(S<RequestAnswer>())
                .Default(QuestionRouting);

            S<RepairAnswer>()
                .Edge(RepairAnswer.NoQuestionForRepair, S<HowCanIHelp>())
                .Edge(RepairAnswer.AdviceAccepted, S<HowCanIHelp>());

            _equivalenceQuestionState = S<EquivalenceQuestion>()
                .YesNoEdge(S<EquivalenceAdvice>(), EquivalenceAdvice.IsEquivalent);

            S<EquivalenceAdvice>()
                .Edge(EquivalenceAdvice.NoEquivalency, S<RequestAnswer>())
                .Default(S<QuestionAnswering>());
        }

        #region State graph building

        private StateGraphBuilder S<StateImplementation>()
            where StateImplementation : StateBase, new()
        {
            var key = typeof(StateImplementation);

            StateGraphBuilder builder;
            if (!_buildersIndex.TryGetValue(key, out builder))
            {
                _buildersIndex[key] = builder = new StateGraphBuilder(new StateImplementation(), _context.Graph);
                _createdBuilders.Add(builder);
            }

            return builder;
        }

        private StateGraphBuilder EntryState<StateImplementation>()
            where StateImplementation : StateBase, new()
        {
            var builder = S<StateImplementation>();
            _currentState = builder;

            return builder;
        }

        private StateGraphBuilder EmptyState(bool isEntryState = false)
        {
            var builder = new StateGraphBuilder(new EmptyState(), _context.Graph);
            if (isEntryState)
                _currentState = builder;
            _createdBuilders.Add(builder);

            return builder;
        }

        private ResponseBase getResponse(string utterance)
        {
            var responses = new List<ModifiableResponse>();

            _context.StartTurn(utterance);
            var edgeInput = new EdgeInput(utterance);
            do
            {
                _context.EdgeReset();

                var scores = edgeInput.GetScore(_currentState);

                Trigger trigger;
                if (scores.Any())
                {
                    var bestHypothesis = scores.First();
                    var confidence = bestHypothesis.Item3;

                    if (confidence < 0.5)
                    {
                        trigger = _currentState.DefaultTrigger;
                    }
                    else if (confidence < 0.9 && _currentState==QuestionRouting)
                    {
                        _context.SetValue(EquivalenceQuestion.QueriedQuestion, utterance);
                        var substitution = substitute(bestHypothesis.Item4, utterance);
                        _context.SetValue(EquivalenceQuestion.PatternQuestion, substitution);
                        edgeInput = new EdgeInput(EquivalenceQuestion.EquivalenceEdge);
                        continue;
                    }
                    else
                    {
                        var substitution = bestHypothesis.Item2;
                        trigger = bestHypothesis.Item1;
                        _context.AddSubstitution(substitution);
                    }
                }
                else
                {
                    trigger = _currentState.DefaultTrigger;
                }

                if (trigger == null)
                    throw new NullReferenceException("Cannot handle null trigger");

                var response = fireTrigger(trigger);
                if (response != null)
                    responses.Add(response);

                edgeInput = _context.CurrentOutput;

            } while (edgeInput != null);

            var concatenation = from response in responses select response.CreateResponse().ToString();
            return new SimpleResponse(string.Join(".", concatenation));
        }

        private string substitute(string pattern, string utterance)
        {
            var patternNodes = QuestionAnsweringModule.GetRelatedNodes(pattern, _context.Graph);
            var utteranceNodes = QuestionAnsweringModule.GetRelatedNodes(utterance, _context.Graph);

            var result = repairSpelling(pattern);
            foreach (var patternNode in patternNodes)
            {
                var nearest = QuestionAnsweringModule.GetNearest(patternNode, utteranceNodes, _context.Graph);
                result = result.Replace(patternNode.Data.ToString(), nearest.Data.ToString());
            }

            return result;
        }

        private string repairSpelling(string utterance)
        {
            var sentence = SentenceParser.Parse(utterance);
            var builder = new StringBuilder();
            foreach (var word in sentence.Words)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                builder.Append(word);
            }

            return builder.ToString();
        }

        private ModifiableResponse fireTrigger(Trigger trigger)
        {
            trigger.Apply(_context);

            _currentState = trigger.TargetNode;
            ConsoleServices.PrintLine(_currentState.State, ConsoleServices.OperatorColor);
            var response = _currentState.ExecuteState(_context);

            return response;
        }

        #endregion

        #region Utterance routing

        public ResponseBase Ask(string question)
        {
            return getResponse(question);
        }

        public ResponseBase Negate()
        {
            return getResponse("no");
        }

        public ResponseBase Advise(string question, string answer)
        {
            //TODO this is hack
            var originalUtterance = question + " is " + answer;

            return getResponse(originalUtterance);
        }

        public ResponseBase Input(string input)
        {
            return getResponse(input);
        }

        #endregion

        public void Close()
        {
            _context.Close();
        }
    }
}
