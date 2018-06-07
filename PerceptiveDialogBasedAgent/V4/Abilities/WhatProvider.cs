using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class WhatProvider : ConceptAbilityBase
    {
        private readonly Concept2 _subjectParameter = Concept2.Subject;

        private readonly Concept2 _propertyParameter = Concept2.Property;

        internal WhatProvider()
            : base("what")
        {
            AddParameter(_propertyParameter);
            AddParameter(_subjectParameter);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var property = generator.GetValue(instance, _propertyParameter);
            var subject = generator.GetValue(instance, _subjectParameter);

            var value = generator.GetValue(subject, property.Concept);
            if (value == null)
            {
                generator.Push(new InformationReportEvent(new ConceptInstance(Concept2.NotFound)));
            }
            else
            {
                generator.Push(new StaticScoreEvent(0.20));
                generator.Push(new InformationReportEvent(value));
            }
        }

        private IEnumerable<ConceptInstance> getRelevantInstances(ConceptInstance instance, Concept2 relevanceCriterion, BeamGenerator beam)
        {
            var relevantCandidates = beam.GetInstances();
            foreach (var relevantCandidate in relevantCandidates)
            {
                if (relevantCandidate == instance)
                    // prevent self reference
                    continue;

                var values = beam.GetPropertyValues(relevantCandidate);
                var isRelevant = values.Any(v => v.Key == relevanceCriterion || v.Value.Concept == relevanceCriterion);

                if (isRelevant)
                    yield return relevantCandidate;
            }
        }
    }
}
