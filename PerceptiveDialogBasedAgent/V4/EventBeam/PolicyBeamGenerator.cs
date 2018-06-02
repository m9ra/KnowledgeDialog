using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class PolicyBeamGenerator : ExecutionBeamGenerator
    {
        private bool _isSubstitutionDisabled = false;

        internal PolicyBeamGenerator()
        {
            DefineConcept(Concept2.YesExplicit);
            //DefineConcept(Concept2.Yes);
            DefineConcept(Concept2.No);
            DefineConcept(Concept2.DontKnow);
            DefineConcept(Concept2.It);

            DefineParameter(Concept2.Prompt, Concept2.Answer, new ConceptInstance(Concept2.Something));
            DefineParameter(Concept2.YesExplicit, Concept2.Subject, new ConceptInstance(Concept2.Something));
            DefineParameter(Concept2.YesExplicit, Concept2.Target, new ConceptInstance(Concept2.Something));


            AddCallback(Concept2.Prompt, _prompt);
            PushToAll(new GoalEvent(new ConceptInstance(Concept2.ActionToExecute)));
        }

        private void policy()
        {
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
            var instanceOfRequest = substitutionRequests.Where(r => r.Target.Instance?.Concept == Concept2.InstanceOf).FirstOrDefault();


            var hasActivatedInstance = activatedInstances.Any();
            var hasResultFound = resultsFound.Any();
            var hasResult = results.Any();
            var hasTooManyResults = tooManyResults.Any();
            var hasUnknownPhrase = unknownPhrases.Any();
            var hasRecentSubstitutionRequest = turnEvents.Any(e => e is SubstitutionRequestEvent);
            var hasSubstitutionRequest = substitutionRequests.Any();
            var hasUnknownPhraseSubstitution = unknownPhraseSubstitutions.Any();
            var hasInstanceOfRequest = instanceOfRequest != null;

            if (hasResultFound)
            {
                //start a new task
                Push(new ActiveInstanceBarrierEvent());
                Push(new CompleteInstanceEvent(resultsFound.First().Instance));
                return;
            }

            if (hasTooManyResults && unknownPhrases.Count() == 1)
            {
                Push(new UnknownPhraseSubstitutionEvent(tooManyResults.First().SubstitutionRequest, unknownPhrases.First()));
                return;
            }

            if (hasTooManyResults)
            {
                var request = tooManyResults.First().SubstitutionRequest;
                Push(new SubstitutionRequestEvent(request.Target));
                return;
            }

            if (hasInstanceOfRequest && unknownPhrases.Count() == 1)
            {
                // throw new NotImplementedException();
                Push(new SubstitutionConfirmationRequestEvent(instanceOfRequest, unknownPhrases.First(), onAccepted: _instanceOfAccepted));
                return;
            }

            if (!hasResult)
            {
                var goal = GetOpenGoal();
                progressGoal(goal);
                return;

                if (hasActivatedInstance)
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
                return;
            }
        }

        private void progressGoal(GoalEvent goalEvt)
        {
            if (goalEvt == null)
            {
                throw new NotImplementedException("How to initialize goal");
            }

            var goalConcept = goalEvt.Goal.Concept;

            if (goalConcept == Concept2.ActionToExecute)
            {
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal override void Visit(GoalEvent evt)
        {
            base.Visit(evt);
            var goalConcept = evt.Goal.Concept;

            if (goalConcept == Concept2.ActionToExecute)
            {
                var target = new PropertySetTarget(evt.Goal, Concept2.Subject);
                Push(new SubstitutionRequestEvent(target));
            }
            else
            {
                throw new NotImplementedException();
            }
        }



        internal override void Visit(SubstitutionConfirmationRequestEvent evt)
        {
            //ask for the confirmation

            Push(new InstanceActivationEvent(null, evt.ConfirmationRequest.Instance));
        }

        private void _instanceOfAccepted(BeamGenerator generator, SubstitutionConfirmationRequestEvent request)
        {
            var target = request.SubstitutionRequest.Target;
            var newConceptName = request.UnknownPhrase.InputPhraseEvt.Phrase;
            var newConcept = generator.DefineConcept(new Concept2(newConceptName, false));

            if (target.Property == Concept2.Subject)
            {
                //we have got a new class member
                var superClass = generator.GetValue(target.Instance, Concept2.Target);
                generator.SetProperty(newConcept, Concept2.InstanceOf, superClass);
            }
            else if (target.Property == Concept2.Target)
            {
                //we have got a new superclass
                var inheritant = generator.GetValue(target.Instance, Concept2.Subject);
                if (inheritant != null)
                    //TODO this should not happen
                    generator.SetProperty(inheritant.Concept, Concept2.InstanceOf, new ConceptInstance(newConcept));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void _prompt(ConceptInstance action, ExecutionBeamGenerator generator)
        {
            var answer = generator.GetValue(action, Concept2.Answer);
            var requester = generator.GetRequester(action);
            if (requester == null)
                throw new InvalidOperationException();

            if (answer.Concept == Concept2.Yes || answer.Concept == Concept2.YesExplicit)
            {
                generator.Push(new StaticScoreEvent(0.5));
                requester.FireOnAccepted(generator);
            }
            else if (answer.Concept == Concept2.No)
            {
                requester.FireOnDeclined(generator);
            }
            else if (answer.Concept == Concept2.DontKnow)
            {
                requester.FireOnUnknown(generator);
            }
            else
            {
                generator.Push(new StaticScoreEvent(-1.0));
            }
        }

        internal override void Visit(TurnEndEvent evt)
        {
            base.Visit(evt);

            _isSubstitutionDisabled = true; //this prevents answering requests/actions before the question was presented to user

            policy();

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
