using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    internal class PhraseStillNotKnownEvent : EventBase
    {
        public readonly UnknownPhraseSubstitutionEvent UnknownPhraseSubstitutionEvent;
        public readonly UnknownPhraseEvent UnknownPhraseEvent;

        public PhraseStillNotKnownEvent(UnknownPhraseSubstitutionEvent unknownPhraseSubstitutionEvent, UnknownPhraseEvent unknownPhraseEvent)
        {
            UnknownPhraseSubstitutionEvent = unknownPhraseSubstitutionEvent;
            UnknownPhraseEvent = unknownPhraseEvent;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[{UnknownPhraseSubstitutionEvent.SubstitutionRequest.Target.TargetRepresentation()}<--{UnknownPhraseSubstitutionEvent.SubstitutionRequest.Target.Property.Name}-- {UnknownPhraseSubstitutionEvent.UnknownPhrase.InputPhraseEvt.Phrase}/{UnknownPhraseEvent.InputPhraseEvt.Phrase}]";
        }
    }
}