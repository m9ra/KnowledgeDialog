using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class PromptAbility : ConceptAbilityBase
    {
        internal PromptAbility() : base(Concept2.Prompt.Name, fireConceptDefinedEvt: false)
        {
            AddParameter(Concept2.Yes);
            AddParameter(Concept2.No);
            AddParameter(Concept2.Answer);
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
