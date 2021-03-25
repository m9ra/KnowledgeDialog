using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class CollectNewConcept : ConceptAbilityBase
    {
        internal CollectNewConcept() : base(Concept2.LearnNewConcept)
        {
            AddParameter(Concept2.Unknown);
            AddParameter(Concept2.Answer);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var unknown = generator.GetValue(instance, Concept2.Unknown);
            var answer = generator.GetValue(instance, Concept2.Answer).Concept;


            if (answer == Concept2.Nothing || answer == Concept2.No || answer == Concept2.Nothing)
            {
                //nothing to report
            }
            else
            {
                generator.Push(new InstanceOutputEvent(instance));
            }
        }

        internal static void GenerateActivationRequest(string unknown, BeamGenerator generator)
        {
            var newConcept = Concept2.From(unknown);
            var newConceptInstance = new ConceptInstance(newConcept);

            var learnInstance = new ConceptInstance(Concept2.LearnNewConcept);
            generator.SetValue(learnInstance, Concept2.Unknown, newConceptInstance);
            generator.Push(new InstanceActivationRequestEvent(learnInstance));
        }
    }
}
