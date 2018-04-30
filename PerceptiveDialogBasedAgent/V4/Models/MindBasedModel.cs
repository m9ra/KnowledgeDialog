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
            _mind.Accept(state.ActiveConcepts, state.PropertyContainer);
            var bestMindState = _mind.BestState;

            MindState newMindState = null;
            if (hasCompleteActions(bestMindState))
            {
                newMindState = executeCompleteActions(bestMindState);
            }
            else if (wasInputUseful(bestMindState))
            {
                newMindState = askForMoreInformation(bestMindState, state);
            }
            else
            {
                //no useful input was given
                newMindState = askExplorativeQuestion(bestMindState, state);
            }

            finalState = BodyState2.Empty();

            _mind.SetBeam(newMindState);

            var output = _nlg.GenerateResponse(newMindState);
            return output;
        }

        private bool wasInputUseful(MindState bestMindState)
        {
            //throw new NotImplementedException();
            return false;
        }

        private MindState askForMoreInformation(MindState bestMindState, BodyState2 state)
        {
            //get the top provider
            throw new NotImplementedException();
        }

        private MindState askExplorativeQuestion(MindState mindState, BodyState2 bodyState)
        {
            //event: input not helpful
            //retry question
            //try picking up some unknown phrases
            throw new NotImplementedException();
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

            V2.Log.Writeln("EVENTS", V2.Log.HeadlineColor);
            V2.Log.Indent();
            foreach (var evt in currentState.Events)
            {
                V2.Log.Writeln(evt.ToString(), V2.Log.ItemColor);
            }
            V2.Log.Dedent();


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
