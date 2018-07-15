using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class OptionPrompt : ConceptAbilityBase
    {
        internal OptionPrompt() : base(Concept2.OptionPrompt.Name, false)
        {
            AddParameter(Concept2.Answer);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var answer = generator.GetValue(instance, Concept2.Answer);
            var options = generator.GetValues(instance, Concept2.Option);
            var disambiguatedOptions = disambiguate(options, answer);

            if (disambiguatedOptions.Count() == 1)
            {
                var activeOption = disambiguatedOptions.First();
                var instanceToActivate = generator.GetValue(activeOption, Concept2.Invocation);
                generator.Push(new StaticScoreEvent(0.1));
                generator.Push(new InstanceActivationRequestEvent(instanceToActivate));
            }
            else if (disambiguatedOptions.Count() == 0)
            {
                generator.Push(new StaticScoreEvent(-0.2));
                //push same options again
                pushNewOptions(options, generator);
            }
            else
            {
                pushNewOptions(disambiguatedOptions, generator);
            }
        }

        internal static ConceptInstance CreatePrompt(Dictionary<Concept2, ConceptInstance> optionEffect, BeamGenerator generator)
        {
            var prompt = new ConceptInstance(Concept2.OptionPrompt);

            foreach (var pair in optionEffect)
            {
                var optionInstance = new ConceptInstance(pair.Key);
                var effect = pair.Value;
                generator.SetValue(optionInstance, Concept2.Invocation, effect);
                generator.SetValue(prompt, Concept2.Option, optionInstance);
            }

            return prompt;
        }

        private void pushNewOptions(IEnumerable<ConceptInstance> options, BeamGenerator generator)
        {
            var optionPrompt = new ConceptInstance(Concept2.OptionPrompt);
            foreach (var option in options)
            {
                generator.SetValue(optionPrompt, Concept2.Option, option);
            }
            generator.Push(new InstanceActivationRequestEvent(optionPrompt));
        }

        private IEnumerable<ConceptInstance> disambiguate(IEnumerable<ConceptInstance> options, ConceptInstance answer)
        {
            var result = new List<ConceptInstance>();
            foreach (var option in options)
            {
                if (option.Concept == answer.Concept)
                    result.Add(option);
            }

            return result;
        }
    }
}
