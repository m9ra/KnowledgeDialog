using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    internal class UnknownPhraseSubstitutedEvent : EventBase
    {
        public readonly UnknownPhraseSubstitutionEvent UnknownPhraseRequest;

        public readonly ConceptInstance SubstitutedValue;

        public UnknownPhraseSubstitutedEvent(UnknownPhraseSubstitutionEvent unknownPhraseRequest, ConceptInstance substitutedValue)
        {
            UnknownPhraseRequest = unknownPhraseRequest;
            SubstitutedValue = substitutedValue;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            var target = UnknownPhraseRequest.SubstitutionRequest.Target;
            var targetDescriptor = target.Concept?.Name ?? target.Instance.Concept.Name;
            return $"[{targetDescriptor}<--{target.Property.Name}--{UnknownPhraseRequest.UnknownPhrase.InputPhraseEvt.Phrase} as {SubstitutedValue.Concept.Name}]";
        }
    }
}