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

            var answer = getAnswer(subject, property.Concept, generator);

            if (answer.Count == 0)
            {
                generator.Push(new InformationReportEvent(new ConceptInstance(Concept2.NotFound)));
            }
            else if (answer.Count == 1)
            {
                generator.Push(new StaticScoreEvent(0.20));
                generator.Push(new InformationReportEvent(answer.First()));
            }
            else
            {
                var needRefinementInstance = new ConceptInstance(Concept2.NeedsRefinement);
                generator.SetValue(needRefinementInstance, Concept2.Subject, subject);
                generator.SetValue(subject, Concept2.OnSetListener, instance);
                generator.Push(new InformationReportEvent(needRefinementInstance));
            }
        }

        private List<ConceptInstance> getAnswer(ConceptInstance subject, Concept2 property, BeamGenerator generator)
        {
            var result = new List<ConceptInstance>();

            var directValue = generator.GetValue(subject, property);
            if (directValue != null)
            {
                result.Add(directValue);
                return result;
            }

            var criterionValues = generator.GetPropertyValues(subject);
            criterionValues.Remove(Concept2.OnSetListener); // TODO internal property removal should be done in more systematic way


            var requiredProperties = new HashSet<Concept2>(criterionValues.Values.Select(i => i.Concept));
            requiredProperties.Add(subject.Concept);

            var relevantConcepts = FindProvider.FindRelevantConcepts(generator, requiredProperties);
            foreach (var concept in relevantConcepts)
            {
                var searchedPropertyValue = generator.GetValue(concept, property);
                if (searchedPropertyValue == null)
                    continue;

                result.Add(searchedPropertyValue);
            }

            return result;
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
