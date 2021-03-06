﻿using System;
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
    public class StateDialogManager : IDialogManager, IInputDialogManager
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
                .YesNoEdge(S<RequestAnswer>(), AcceptAdvice.IsBasedOnContextProperty)
                .Edge(RequestContext.HasContextAnswerEdge, S<RequestAnswer>())
                .Default(S<ApologizeState>());

            S<RequestAnswer>()
                .IsEdge(S<AcceptAdvice>(), AcceptAdvice.CorrectAnswerProperty)
                .Edge(RequestAnswer.HasCorrectAnswerEdge, S<AcceptAdvice>())
                .Default(S<ApologizeState>());

            S<AcceptAdvice>()
                //when info is missing, we have to go back to advice routing
                .Edge(AcceptAdvice.MissingInfoEdge, S<RequestContext>())
                //when advice handling is completed go to QuestionRouting
                .Default(QuestionRouting);

            QuestionRouting
                //we know requested question
                .HasMatch(_context.QuestionAnsweringModule.Triggers, S<QuestionAnswering>(), RequestAnswer.QuestionProperty)

                .Edge(EquivalenceQuestion.EquivalenceEdge, S<EquivalenceQuestion>())

                .IsEdge(S<AcceptAdvice>(), AcceptAdvice.CorrectAnswerProperty)

                //question is not recognized as advice
                .Default(S<RequestContext>(), RequestAnswer.QuestionProperty);

            S<QuestionAnswering>()
                .TakeEdgesFrom(S<RequestAnswer>())
                .Default(QuestionRouting);

            S<RepairAnswer>()
                .Edge(RepairAnswer.NoQuestionForRepair, S<HowCanIHelp>())
                .Edge(RepairAnswer.AdviceAccepted, S<HowCanIHelp>());

            _equivalenceQuestionState = S<EquivalenceQuestion>()
                .YesNoEdge(S<EquivalenceAdvice>(), EquivalenceAdvice.IsEquivalent)
                .Default(S<ApologizeState>())
                ;

            S<EquivalenceAdvice>()
                .Edge(EquivalenceAdvice.NoEquivalency, S<RequestContext>())
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
            return getResponse(UtteranceParser.Parse(utterance));
        }

        private ResponseBase getResponse(ParsedUtterance utterance)
        {
            var responses = new List<ModifiableResponse>();
            var originalUtterance = utterance.OriginalSentence;

            _context.StartTurn(originalUtterance);
            var edgeInput = new EdgeInput(originalUtterance);
            do
            {
                _context.EdgeReset();

                var scores = edgeInput.GetScore(_currentState);

                Trigger trigger;
                if (scores.Any())
                {
                    var bestHypothesis = scores.First();
                    var score = bestHypothesis.Score;
                    var nonPatternQuestion = getBestNonPattern(scores);

                    if (score < 0.5)
                    {
                        trigger = _currentState.DefaultTrigger;
                    }
                    else if (nonPatternQuestion != null && nonPatternQuestion.Score < 0.9 && _currentState == QuestionRouting)
                    {
                        _context.SetValue(EquivalenceQuestion.QueriedQuestion, originalUtterance);
                        var substitution = substitute(nonPatternQuestion.ParsedSentence, utterance);
                        _context.SetValue(EquivalenceQuestion.PatternQuestion, substitution);
                        edgeInput = new EdgeInput(EquivalenceQuestion.EquivalenceEdge);
                        continue;
                    }
                    else if (score < 0.9)
                    {
                        trigger = _currentState.DefaultTrigger;
                    }
                    else
                    {
                        var substitution = bestHypothesis.Substitution;
                        trigger = bestHypothesis.Value;
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

        private MappingControl<Trigger> getBestNonPattern(IEnumerable<MappingControl<Trigger>> hypotheses)
        {
            foreach (var hypothesis in hypotheses)
            {
                if (hypothesis.ParsedSentence != null && !hypothesis.ParsedSentence.OriginalSentence.Contains('*'))
                    return hypothesis;
            }
            return null;
        }

        private string substitute(ParsedUtterance pattern, ParsedUtterance utterance)
        {
            var patternNodes = _context.QuestionAnsweringModule.GetPatternNodes(pattern);
            var utteranceNodes = _context.QuestionAnsweringModule.GetRelatedNodes(utterance).ToArray();
            var substitutions = HeuristicQAModule.GetSubstitutions(utteranceNodes, patternNodes, _context.Graph);

            var result = repairSpelling(pattern.OriginalSentence);
            foreach (var patternNode in patternNodes)
            {
                NodeReference substitution;
                if (!substitutions.TryGetValue(patternNode, out substitution))
                    substitution = patternNode;

                result = result.Replace(patternNode.Data.ToString(), substitution.Data.ToString());
            }

            return result;
        }

        private string repairSpelling(string utterance)
        {
            var sentence = UtteranceParser.Parse(utterance);
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

        public ResponseBase Input(ParsedUtterance utterance)
        {
            return getResponse(utterance);
        }

        #endregion

        public void Close()
        {
            _context.Close();
        }

        public ResponseBase Initialize()
        {
            return new SimpleResponse("Hello, how can I help you?");
        }
    }
}
