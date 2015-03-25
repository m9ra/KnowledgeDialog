using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.PoolComputation.PoolActions;

using KnowledgeDialog.PoolComputation.Frames;
using KnowledgeDialog.PoolComputation.ModifiableResponses;

using KnowledgeDialog.PoolComputation.StateDialog;
using KnowledgeDialog.PoolComputation.StateDialog.States;

namespace KnowledgeDialog.PoolComputation
{
    class StateDialogManager : IDialogManager
    {
        private readonly StateContext _context;

        private readonly Dictionary<Type, StateGraphBuilder> _buildersIndex = new Dictionary<Type, StateGraphBuilder>();

        private readonly List<StateGraphBuilder> _createdBuilders = new List<StateGraphBuilder>();

        private StateGraphBuilder _currentState = null;

        public StateDialogManager(params GraphLayerBase[] layers)
        {
            _context = new StateContext(new ComposedGraph(layers));


            S<Welcome>()
                .Default(S<HowCanIHelp>());

            var QuestionRouting = EmptyState(true);
            S<HowCanIHelp>()
                .Default(QuestionRouting);

            S<AdviceRouting>()
                .Edge("it is *", S<AcceptAdvice>(), AcceptAdvice.CorrectAnswerProperty)
                .Edge("* is the correct answer", S<AcceptAdvice>(), AcceptAdvice.CorrectAnswerProperty)
                .YesNoEdge(S<AcceptAdvice>(), AcceptAdvice.IsBasedOnContextProperty)
                .Default(S<AdviceRouting>());

            S<AcceptAdvice>()
                //when info is missing, we have to go back to advice routing
                .Edge(AcceptAdvice.MissingInfoEdge, S<AdviceRouting>())
                //when advice handling is completed go to QuestionRouting
                .Default(QuestionRouting);

            QuestionRouting
                //we know requested question
                .HasMatch(_context.QuestionAnsweringModule.Triggers, S<QuestionAnswering>(), AdviceRouting.QuestionProperty)

                .Edge("it is *", S<RepairAnswer>(), AcceptAdvice.CorrectAnswerProperty)
                .Edge("* is the correct answer", S<RepairAnswer>(), AcceptAdvice.CorrectAnswerProperty)

                //question is not recognized as advice
                .Default(S<AdviceRouting>(), AdviceRouting.QuestionProperty);

            S<QuestionAnswering>()
                .TakeEdgesFrom(S<AdviceRouting>())
                .Default(QuestionRouting);

            S<RepairAnswer>()
                .Edge(RepairAnswer.NoQuestionForRepair, S<HowCanIHelp>())
                .Edge(RepairAnswer.AdviceAccepted, S<HowCanIHelp>());
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
                    else if (confidence < 0.9)
                    {
                        throw new NotImplementedException("Ask for equivalence");
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

        #endregion
    }
}
