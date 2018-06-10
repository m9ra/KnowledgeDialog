using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class PolicyBeamGenerator : AbilityBeamGenerator
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
            PushToAll(new FrameEvent(new ConceptInstance(Concept2.ActionToExecute)));
        }

        private void policy()
        {
            // collect turn features
            var previousTurnEvents = GetPreviousTurnEvents();
            var turnEvents = GetTurnEvents();
            var resultsFound = turnEvents.Select(e => e as InformationReportEvent).Where(e => e != null).ToArray();
            var activatedInstances = turnEvents.Select(e => e as InstanceActivationRequestEvent).Where(e => e != null).ToArray();
            var tooManyResults = new EventBase[0]; //TODO!!!
            var unknownPhrases = new EventBase[0]; //TODO!!!
            var unknownPhraseSubstitutions = new EventBase[0];//TODO!!!
            var results = turnEvents.Select(e => e as InformationReportEvent).Where(e => e != null).ToArray();
            var substitutionRequests = GetAvailableSubstitutionRequests();
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
                throw new NotImplementedException("Start a new frame");
                return;
            }

            if (hasTooManyResults)
            {
                throw new NotImplementedException("Create frame for too many results found");
                return;
            }
        }

        private void progressGoal(FrameEvent goalEvt)
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

        internal override void Visit(FrameEvent evt)
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


        private void _instanceOfAccepted(BeamGenerator generator, object request)
        {
            PropertySetTarget target = null;
            string newConceptName = null;
            var newConcept = generator.DefineConcept(Concept2.From(newConceptName, false));

            if (target.Property == Concept2.Subject)
            {
                //we have got a new class member
                var superClass = generator.GetValue(target.Instance, Concept2.Target);
                generator.SetValue(newConcept, Concept2.InstanceOf, superClass);
            }
            else if (target.Property == Concept2.Target)
            {
                //we have got a new superclass
                var inheritant = generator.GetValue(target.Instance, Concept2.Subject);
                if (inheritant != null)
                    //TODO this should not happen
                    generator.SetValue(inheritant.Concept, Concept2.InstanceOf, new ConceptInstance(newConcept));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void _prompt(ConceptInstance action, BeamGenerator generator)
        {
            var answer = generator.GetValue(action, Concept2.Answer);
            throw new NotImplementedException("Process prompt");
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

    }
}
