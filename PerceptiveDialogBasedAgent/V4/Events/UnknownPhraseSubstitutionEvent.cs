
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    internal class UnknownPhraseSubstitutionEvent : EventBase
    {
        internal readonly SubstitutionRequestEvent SubstitutionRequest;

        internal readonly UnknownPhraseEvent UnknownPhrase;

        public UnknownPhraseSubstitutionEvent(SubstitutionRequestEvent substitutionRequest, UnknownPhraseEvent unknownPhrase)
        {
            SubstitutionRequest = substitutionRequest;
            UnknownPhrase = unknownPhrase;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }
    }
}