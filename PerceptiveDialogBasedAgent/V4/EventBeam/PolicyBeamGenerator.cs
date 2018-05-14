using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class PolicyBeamGenerator : ExecutionBeamGenerator
    {
        internal override void Visit(TurnEndEvent evt)
        {
            base.Visit(evt);

            // collect turn features
            var turnEvents = GetTurnEvents();
            var resultsFound = turnEvents.Select(e => e as InstanceFoundEvent).Where(e => e != null).ToArray();
            var tooManyResults = turnEvents.Select(e => e as TooManyInstancesFoundEvent).Where(e => e != null).ToArray();
            var unknownPhrases = turnEvents.Select(e => e as UnknownPhraseEvent).Where(e => e != null).ToArray();
            var results = turnEvents.Select(e => e as ResultEvent).Where(e => e != null).ToArray();
            var substitutionRequests = GetFreeSubstitutionRequests();


            var hasResultFound = resultsFound.Any();
            var hasResult = results.Any();
            var hasTooManyResults = tooManyResults.Any();
            var hasUnknownPhrase = unknownPhrases.Any();
            var hasRecentSubstitutionRequest = turnEvents.Any(e => e is SubstitutionRequestEvent);
            var hasSubstitutionRequest = substitutionRequests.Any();

            if (hasResultFound)
            {
                //start a new task
                Push(new ActiveInstanceBarrierEvent());
                Push(new CompleteInstanceEvent(resultsFound.First().Instance));
            }
            else if (hasTooManyResults && hasUnknownPhrase)
            {
                Push(new UnknownPhraseSubstitutionEvent(tooManyResults.First().SubstitutionRequest, unknownPhrases.First()));
            }
            else if (hasTooManyResults)
            {
                var request = tooManyResults.First().SubstitutionRequest;
                Push(new SubstitutionRequestEvent(request.Target));
            }
            else if (!hasResult && hasUnknownPhrase && !hasRecentSubstitutionRequest && hasSubstitutionRequest)
            {
                Push(new UnknownPhraseSubstitutionEvent(substitutionRequests.First(), unknownPhrases.First()));
            }
        }

        internal override void Visit(UnknownPhraseSubstitutedEvent evt)
        {
            base.Visit(evt);

            Push(new ConceptDescriptionEvent(evt.SubstitutedValue.Concept, evt.UnknownPhraseRequest.UnknownPhrase.InputPhraseEvt.Phrase));
        }
        
    }
}
