using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Abilities;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class LearnNewPhrase : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var unknownPhrases = getUnknownPhrases(generator).ToArray();
            if (unknownPhrases.Length == 0)
                yield break;

            var phraseToAsk = unknownPhrases.First();
            var newConcept = Concept2.From(phraseToAsk);
            var newConceptInstance = new ConceptInstance(newConcept);


            var handlePropertyInstance = new ConceptInstance(Concept2.AcceptNewProperty);
            generator.SetValue(handlePropertyInstance, Concept2.Property, newConceptInstance);

            var handleConceptInstance = new ConceptInstance(Concept2.Nothing);

            var options = new Dictionary<Concept2, ConceptInstance>()
            {
                {Concept2.Property, handlePropertyInstance},
                {Concept2.ConceptName, handleConceptInstance}
            };

            var prompt = OptionPrompt.CreatePrompt(options, generator);
            generator.Push(new InstanceActivationRequestEvent(prompt));

            yield return $"What does '{unknownPhrases.First()}' mean?";
        }

        private IEnumerable<string> getUnknownPhrases(BeamGenerator generator)
        {
            var allInputPhrases = GetMany<InputPhraseEvent>();

            var currentBuffer = new List<InputPhraseEvent>();
            foreach (var inputPhrase in allInputPhrases)
            {
                var phrase = inputPhrase;
                currentBuffer.Add(phrase);

                if (generator.IsInputUsed(phrase) || isDelimiter(inputPhrase))
                {
                    if (currentBuffer.Count > 0)
                        yield return composeUnknownPhrase(currentBuffer);

                    currentBuffer.Clear();
                }
            }

            if (currentBuffer.Count > 0)
                yield return composeUnknownPhrase(currentBuffer);
        }

        private bool isDelimiter(InputPhraseEvent inputPhrase)
        {
            var phrase = inputPhrase.Phrase;
            return new[] { "in", "from", "out", "of" }.Contains(phrase);
        }

        private string composeUnknownPhrase(IEnumerable<InputPhraseEvent> currentBuffer)
        {
            return string.Join(" ", currentBuffer.Select(i => i.Phrase));
        }
    }
}
