using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    internal delegate void BeamRequestAction(BeamGenerator generator, SubstitutionConfirmationRequestEvent request);

    class SubstitutionConfirmationRequestEvent : EventBase
    {
        public readonly SubstitutionRequestEvent SubstitutionRequest;

        public readonly UnknownPhraseEvent UnknownPhrase;

        public readonly PropertySetTarget ConfirmationRequest;

        private readonly BeamRequestAction OnAccepted;

        private readonly BeamRequestAction OnDeclined;

        private readonly BeamRequestAction OnUnknown;

        public SubstitutionConfirmationRequestEvent(SubstitutionRequestEvent substitutionRequest, UnknownPhraseEvent unknownPhrase, BeamRequestAction onAccepted = null, BeamRequestAction onDeclined = null, BeamRequestAction onUnknown = null)
        {
            SubstitutionRequest = substitutionRequest;
            UnknownPhrase = unknownPhrase;

            ConfirmationRequest = new PropertySetTarget(new ConceptInstance(Concept2.Prompt), Concept2.Answer);

            OnAccepted = onAccepted;
            OnDeclined = onDeclined;
            OnUnknown = onUnknown;
        }

        internal void FireOnAccepted(BeamGenerator generator)
        {
            OnAccepted?.Invoke(generator, this);
            generator.Push(new ConfirmationAcceptedEvent());
        }

        internal void FireOnDeclined(BeamGenerator generator)
        {
            OnDeclined?.Invoke(generator, this);
        }

        internal void FireOnUnknown(BeamGenerator generator)
        {
            OnUnknown?.Invoke(generator, this);
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            var target = SubstitutionRequest.Target;
            return $"[yes/no {target.Instance.Concept.Name}<--{target.Property.Name}--\"{UnknownPhrase.InputPhraseEvt.Phrase}\"]";
        }
    }
}
