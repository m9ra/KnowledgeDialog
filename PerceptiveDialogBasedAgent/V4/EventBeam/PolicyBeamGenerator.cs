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
        private bool _isSubstitutionDisabled = false;

        internal override void Visit(TurnEndEvent evt)
        {
            base.Visit(evt);

            // collect turn features
            var previousTurnEvents = GetPreviousTurnEvents();
            var turnEvents = GetTurnEvents();
            var resultsFound = turnEvents.Select(e => e as InstanceFoundEvent).Where(e => e != null).ToArray();
            var activatedInstances = turnEvents.Select(e => e as InstanceActivationEvent).Where(e => e != null).ToArray();
            var tooManyResults = turnEvents.Select(e => e as TooManyInstancesFoundEvent).Where(e => e != null).ToArray();
            var unknownPhrases = turnEvents.Select(e => e as UnknownPhraseEvent).Where(e => e != null).ToArray();
            var unknownPhraseSubstitutions = GetFreeUnknownPhraseRequests();
            var results = turnEvents.Select(e => e as ResultEvent).Where(e => e != null).ToArray();
            var substitutionRequests = GetFreeSubstitutionRequests();


            var hasActiveInstance = activatedInstances.Any();
            var hasResultFound = resultsFound.Any();
            var hasResult = results.Any();
            var hasTooManyResults = tooManyResults.Any();
            var hasUnknownPhrase = unknownPhrases.Any();
            var hasRecentSubstitutionRequest = turnEvents.Any(e => e is SubstitutionRequestEvent);
            var hasSubstitutionRequest = substitutionRequests.Any();
            var hasUnknownPhraseSubstitution = unknownPhraseSubstitutions.Any();

            _isSubstitutionDisabled = true; //this prevents answering requests/actions before the question was presented to user
            if (hasResultFound)
            {
                //start a new task
                Push(new ActiveInstanceBarrierEvent());
                Push(new CompleteInstanceEvent(resultsFound.First().Instance));
            }
            else if (hasTooManyResults && unknownPhrases.Count() == 1)
            {
                Push(new UnknownPhraseSubstitutionEvent(tooManyResults.First().SubstitutionRequest, unknownPhrases.First()));
            }
            else if (hasTooManyResults)
            {
                var request = tooManyResults.First().SubstitutionRequest;
                Push(new SubstitutionRequestEvent(request.Target));
            }
            else if (!hasResult)
            {
                if (hasActiveInstance)
                {
                    Push(new InstanceUnderstoodEvent(activatedInstances.First()));
                }
                else if (hasUnknownPhrase && !hasRecentSubstitutionRequest && hasSubstitutionRequest)
                {
                    Push(new UnknownPhraseSubstitutionEvent(substitutionRequests.First(), unknownPhrases.First()));
                }
                else if (hasUnknownPhrase && hasUnknownPhraseSubstitution)
                {
                    Push(new PhraseStillNotKnownEvent(unknownPhraseSubstitutions.First(), unknownPhrases.First()));
                }
            }

            _isSubstitutionDisabled = false;
        }

        internal override void Visit(SubstitutionRequestEvent evt)
        {
            if (!_isSubstitutionDisabled)
                base.Visit(evt);
        }

        internal override void Visit(UnknownPhraseSubstitutedEvent evt)
        {
            base.Visit(evt);

            Push(new ConceptDescriptionEvent(evt.SubstitutedValue.Concept, evt.UnknownPhraseRequest.UnknownPhrase.InputPhraseEvt.Phrase));
        }

    }
}
