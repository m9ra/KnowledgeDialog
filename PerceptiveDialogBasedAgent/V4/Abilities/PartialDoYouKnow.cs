using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class PartialDoYouKnow : ConceptAbilityBase
    {
        internal PartialDoYouKnow() : base("partial do you know")
        {
            Description("do you know");
            Description("do know");
            Description("does");
            Description("you know that");
            AddParameter(Concept2.Subject);
            AddParameter(Concept2.Property);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var subject = generator.GetValue(instance, Concept2.Subject);
            var property = generator.GetValue(instance, Concept2.Property);
            var inheritedProperties = generator.GetPropertyValues(subject, includeInstanceProps: false);

            var value = generator.GetValue(subject, property.Concept);
            if (value == null)
            {
                var refutation = new ConceptInstance(Concept2.KnowledgeRefutation);
                var refutedInfo = new ConceptInstance(subject.Concept);
                generator.SetValue(refutedInfo, property.Concept, new ConceptInstance(Concept2.Something));
                generator.SetValue(refutation, Concept2.Subject, refutedInfo);
                generator.SetValue(refutation, Concept2.Property, property);
                generator.Push(new InformationReportEvent(refutation));
            }
            else
            {
                var confirmation = new ConceptInstance(Concept2.KnowledgeConfirmed);
                var confirmedInfo = new ConceptInstance(subject.Concept);

                generator.SetValue(confirmedInfo, property.Concept, value);
                generator.SetValue(confirmation, Concept2.Subject, confirmedInfo);
                generator.Push(new InformationReportEvent(confirmation));
            }
        }
    }
}
