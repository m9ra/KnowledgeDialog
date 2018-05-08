using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Brain;

namespace PerceptiveDialogBasedAgent.V4.Models
{
    internal class MindBasedModel : HandcraftedModel
    {
        private readonly Mind _mind;

        private readonly EventBasedNLG _nlg;

        private readonly ConceptInstance _actionRequester;

        internal MindBasedModel(Body body) : base(body)
        {
            _nlg = new EventBasedNLG(body);
            _actionRequester = body.RootConcept;
            var mindState = MindState.Empty(body, _actionRequester);

            _mind = new Mind();
            _mind.SetBeam(mindState);
        }

        internal override string StateReaction(BodyState2 state, out BodyState2 finalState)
        {
            LogState(state);

            _mind.NewTurnEvent();
            var newMindState = ProcessState(state, _mind);
            newMindState = ProcessInvocation(newMindState);

            if (!wasInputUseful(newMindState))
            {
                //no useful input was given
                newMindState = askExplorativeQuestion(newMindState, state);
            }

            V2.Log.Writeln("EVENTS", V2.Log.HeadlineColor);
            V2.Log.Indent();
            foreach (var evt in newMindState.Events)
            {
                V2.Log.Writeln(evt.ToString(), V2.Log.ItemColor);
            }
            V2.Log.Dedent();

            finalState = BodyState2.Empty();

            _mind.SetBeam(newMindState);

            var output = _nlg.GenerateResponse(newMindState);
            return output;
        }

        internal MindState ProcessInvocation(MindState state)
        {
            var events = state.Events.ToArray();

            var newState = state;
            for (var i = events.Length - 1; i >= 0; --i)
            {
                var evt = events[i];
                if (evt.Concept == Concept2.NewTurn)
                    break;

                if (evt.Concept != Concept2.Invocation)
                    continue;

                var invokedConcept = state.GetPropertyValue(evt, Concept2.Subject) as ConceptInstance;
                var context = new MindEvaluationContext(invokedConcept, newState);
                newState = context.EvaluateOnExecution();
            }

            return newState;
        }

        internal MindState ProcessState(BodyState2 state, Mind mind)
        {
            mind.Accept(state.ActiveConcepts, state.PropertyContainer);
            var bestMindState = mind.BestState;

            var newMindState = bestMindState;
            if (hasCompleteActions(bestMindState))
            {
                newMindState = executeCompleteActions(bestMindState);
            }


            return newMindState;
        }

        private bool wasInputUseful(MindState bestMindState)
        {
            return bestMindState.Events.LastOrDefault().Concept != Concept2.NewTurn;
        }

        private MindState askExplorativeQuestion(MindState mindState, BodyState2 bodyState)
        {
            var unknownPhrases = HandcraftedModel.GetUnknownPhrases(bodyState).ToArray();
            if (!unknownPhrases.Any())
                throw new NotImplementedException();

            var phrase = unknownPhrases.First();
            var descriptionRequest = new ConceptInstance(_body.AcceptDescriptionAction);
            var evt = new ConceptInstance(Concept2.SubstitutionRequestedEvent);

            mindState = mindState.SetPropertyValue(descriptionRequest, Concept2.StateToRetry, new MindStateInstance(mindState));
            mindState = mindState.SetPropertyValue(descriptionRequest, Concept2.ConceptName, Phrase.FromUtterance(phrase));

            mindState = mindState.SetPropertyValue(evt, Concept2.Target, descriptionRequest);
            mindState = mindState.SetPropertyValue(evt, Concept2.TargetProperty, Concept2.Subject);
            mindState = mindState.AddEvent(evt);
            return mindState;
        }

        private MindState executeCompleteActions(MindState bestMindState)
        {
            var currentState = bestMindState;
            var actions = getCompleteActions(currentState).ToArray();
            foreach (var action in actions)
            {
                V2.Log.Writeln("Executing: " + action, V2.Log.ExecutedCommandColor);
                if (action.Concept.OnExecution == null)
                    continue;

                var context = new MindEvaluationContext(action, currentState);
                currentState = context.EvaluateOnExecution();
            }

            return currentState.SetPropertyValue(_actionRequester, Concept2.CompleteAction, (PointableInstance)null);
        }

        private bool hasCompleteActions(MindState state)
        {
            return getCompleteActions(state).Any();
        }

        private IEnumerable<ConceptInstance> getCompleteActions(MindState state)
        {
            var action = state.GetPropertyValue(_actionRequester, Concept2.CompleteAction) as ConceptInstance;
            if (action != null)
                yield return action;
        }
    }
}
