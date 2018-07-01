using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class DoYouKnow : ConceptAbilityBase
    {
        internal DoYouKnow() : base("do you know")
        {
            AddParameter(Concept2.Subject);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var subject = generator.GetValue(instance, Concept2.Subject);
            var inheritedProperties = generator.GetPropertyValues(subject, includeInstanceProps: false);
            var pureInstanceProperties = generator.GetPropertyValues(subject, includeInheritedProps: false);
            if (!pureInstanceProperties.Any())
            {
                var confirmation = new ConceptInstance(Concept2.KnowledgeConfirmed);
                generator.SetValue(confirmation, Concept2.Subject, subject);
                generator.Push(new InformationReportEvent(confirmation));
                return;
            }

            var unknown = new List<KeyValuePair<Concept2, ConceptInstance>>();
            foreach (var propertyValue in pureInstanceProperties)
            {
                if (!inheritedProperties.TryGetValue(propertyValue.Key, out var knownValue) || knownValue.Concept != propertyValue.Value.Concept)
                    unknown.Add(propertyValue);
            }

            if (unknown.Count == 0)
            {
                var confirmation2 = new ConceptInstance(Concept2.KnowledgeConfirmed);
                generator.SetValue(confirmation2, Concept2.Subject, subject);
                generator.Push(new InformationReportEvent(confirmation2));
                return;
            }

            var unknownReport = new ConceptInstance(subject.Concept);
            foreach(var propertyValue in unknown)
            {
                generator.SetValue(unknownReport, propertyValue.Key, propertyValue.Value);
            }

            var confirmation3 = new ConceptInstance(Concept2.KnowledgeRefutation);
            generator.SetValue(confirmation3, Concept2.Subject, subject);
            generator.Push(new InformationReportEvent(unknownReport));
        }
    }
}
