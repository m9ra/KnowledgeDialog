using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Abilities;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class RequestNewPropertyExplanation : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var unknownPhrases = getUnknownPhrases(generator).ToArray();

            var substitutionRequest = Get<InformationPartEvent>(p => !p.IsFilled, searchInsideTurnOnly: false);
            if (PreviousPolicy<LearnNewPhrase>(out _) || unknownPhrases.Count() != 1 || substitutionRequest == null || substitutionRequest.Subject == null)
                yield break;

            var unknownPhrase = unknownPhrases.FirstOrDefault();

            var unknownPropertyCandidate = new ConceptInstance(Concept2.From(unknownPhrase));
            var newPropertyAssignment = Find<PropertySetEvent>(p => p.Target.Property == Concept2.HasProperty && p.SubstitutedValue?.Concept == substitutionRequest.Property, precedingTurns: 1);
            if (newPropertyAssignment != null)
            {
                //in the previous turn, new property was registered - this might be its value
                var remember = RememberPropertyValue.Create(generator, new PropertySetTarget(substitutionRequest.Subject, substitutionRequest.Property), unknownPropertyCandidate);
                YesNoPrompt.Generate(generator, remember, new ConceptInstance(Concept2.Nothing));

                yield return $"So, you think {singular(substitutionRequest.Subject)} {singular(substitutionRequest.Property)} {unknownPhrase} ?";
                yield break;
            }

            // Unknown value when substitution is required was observed
            // TODO detect whether request is for parameter (then nothing to do here)
            // or try to learn new property value
            /*
            var assignUnknownProperty = new ConceptInstance(Concept2.AssignUnknownProperty);
            generator.SetValue(assignUnknownProperty, Concept2.Subject, unknownPropertyCandidate);

            //TODO incorporate target property
            generator.SetValue(assignUnknownProperty, Concept2.Target, substitutionRequest.Subject);
            generator.Push(new InstanceActivationRequestEvent(assignUnknownProperty));

            yield return $"What does {unknownPhrase} mean?";
            */
        }

        private IEnumerable<string> getUnknownPhrases(BeamGenerator generator)
        {
            var allInputPhrases = GetMany<InputPhraseEvent>();

            var currentBuffer = new List<InputPhraseEvent>();
            foreach (var inputPhrase in allInputPhrases)
            {
                var phrase = inputPhrase;
                if (generator.IsInputUsed(phrase))
                {
                    if (currentBuffer.Count > 0)
                        yield return composeUnknownPhrase(currentBuffer);

                    currentBuffer.Clear();
                }

                currentBuffer.Add(phrase);
            }

            if (currentBuffer.Count > 0)
                yield return composeUnknownPhrase(currentBuffer);
        }

        private string composeUnknownPhrase(IEnumerable<InputPhraseEvent> currentBuffer)
        {
            return string.Join(" ", currentBuffer.Select(i => i.Phrase));
        }
    }
}
