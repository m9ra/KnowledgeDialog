using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class YesNoPrompt : ConceptAbilityBase
    {
        internal YesNoPrompt() : base(Concept2.Prompt.Name, fireConceptDefinedEvt: false)
        {
            AddParameter(Concept2.Yes);
            AddParameter(Concept2.No);
            AddParameter(Concept2.Answer);
        }

        internal static void Generate(BeamGenerator generator, ConceptInstance yesInstance, ConceptInstance noInstance)
        {
            var prompt = Create(generator, yesInstance, noInstance);
            generator.Push(new InstanceActivationRequestEvent(prompt));
        }

        internal static ConceptInstance Create(BeamGenerator generator, ConceptInstance yesInstance, ConceptInstance noInstance)
        {
            var prompt = new ConceptInstance(Concept2.Prompt);
            generator.SetValue(prompt, Concept2.Yes, yesInstance);
            generator.SetValue(prompt, Concept2.No, noInstance);
            return prompt;
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var answer = generator.GetValue(instance, Concept2.Answer).Concept;
            if (answer == Concept2.Yes)
            {
                var yesInstance = generator.GetValue(instance, Concept2.Yes);
                generator.Push(new InstanceActivationRequestEvent(yesInstance));
            }
            else if (answer == Concept2.No)
            {
                var noInstance = generator.GetValue(instance, Concept2.No);
                generator.Push(new InstanceActivationRequestEvent(noInstance));
            }
            else
            {
                generator.Push(new StaticScoreEvent(-1));
            }
        }
    }
}
