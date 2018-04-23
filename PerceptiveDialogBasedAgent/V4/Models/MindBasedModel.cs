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

        private readonly EventBasedNLG _nlg = new EventBasedNLG();

        private readonly ActionManagerPlanProvider _actionRequester;

        internal MindBasedModel(Body body) : base(body)
        {
            _actionRequester = new ActionManagerPlanProvider(body);

            var mindState = MindState.Empty();
            mindState = mindState.AddPlanProvider(_actionRequester);

            _mind = new Mind();
            _mind.SetBeam(mindState);
        }

        internal override string StateReaction(BodyState2 state, out BodyState2 finalState)
        {
            LogState(state);

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
                newMindState = askForMissingInformation(bestMindState, state);
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
            var provider = bestMindState.GetActivePlanProvider();
            return provider.GenerateQuestion(bestMindState);
        }

        private MindState askForMissingInformation(MindState mindState, BodyState2 bodyState)
        {
            //event: input not helpful
            //retry question
            //try picking up some unknown phrases
            throw new NotImplementedException();
        }

        private MindState executeCompleteActions(MindState bestMindState)
        {
            var actions = getCompleteActions(bestMindState);
            foreach (var action in actions)
            {
                V2.Log.Writeln("TODO Missing execution for: " + action);
            }

            return bestMindState.SetValue<ConceptInstance>(_actionRequester.CompleteActions, null);
        }

        private bool hasCompleteActions(MindState state)
        {
            return getCompleteActions(state).Any();
        }

        private IEnumerable<ConceptInstance> getCompleteActions(MindState state)
        {
            var actions = state.GetValue(_actionRequester.CompleteActions);
            if (actions == null)
                return Enumerable.Empty<ConceptInstance>();

            return actions;
        }
    }
}
