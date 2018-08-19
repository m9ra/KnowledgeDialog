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
            var unknownPhrases = GetUnknownPhrases(generator).ToArray();
            if (unknownPhrases.Length != 1)
                yield break;

            var phraseToAsk = unknownPhrases.First();
            if (phraseToAsk.Split(' ').Length > 2)
                yield break;

            var newConcept = Concept2.From(phraseToAsk);
            var newConceptInstance = new ConceptInstance(newConcept);

            var handlePropertyInstance = new ConceptInstance(Concept2.AcceptNewProperty);
            generator.SetValue(handlePropertyInstance, Concept2.Property, newConceptInstance);

            var handleConceptInstance = new ConceptInstance(Concept2.Nothing);
            var options = new Dictionary<Concept2, ConceptInstance>()
            {
                {Concept2.Property, handlePropertyInstance},
                {Concept2.ConceptName, handleConceptInstance},
            };

            var prompt = OptionPrompt.CreatePrompt(options, generator);

            //remember runtime info so others can use it
            generator.SetValue(TagInstance, Concept2.Unknown, newConceptInstance);
            generator.SetValue(TagInstance, Concept2.Prompt, prompt);

            generator.Push(new InstanceActivationRequestEvent(prompt));

            yield return $"What does '{unknownPhrases.First()}' mean?";
        }
    }
}
