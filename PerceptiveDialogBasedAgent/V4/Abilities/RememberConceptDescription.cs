using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class RememberConceptDescription : ConceptAbilityBase
    {
        internal RememberConceptDescription() : base(Concept2.RememberConceptDescription, fireConceptDefinedEvt: false)
        {
            AddParameter(Concept2.Subject);
            AddParameter(Concept2.Description);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var subject = generator.GetValue(instance, Concept2.Subject);
            var description = generator.GetValue(instance, Concept2.Description);

            var conceptDescription = new ConceptDescriptionEvent(subject.Concept, description.Concept.Name);
            var export = new ExportEvent(conceptDescription);
            generator.Push(export);
            generator.Push(conceptDescription);
        }

        internal static void Activate(Concept2 subject, string description, BeamGenerator generator)
        {
            var instance = Create(generator, subject, description);

            generator.Push(new InstanceActivationRequestEvent(instance));
        }

        internal static ConceptInstance Create(BeamGenerator generator, Concept2 subject, string description)
        {
            var instance = new ConceptInstance(Concept2.RememberConceptDescription);

            generator.SetValue(instance, Concept2.Subject, new ConceptInstance(subject));
            generator.SetValue(instance, Concept2.Description, new ConceptInstance(Concept2.From(description)));
            return instance;
        }
    }
}
