using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class FindProvider : ConceptAbilityBase
    {
        private readonly Concept2 _parameter = Concept2.Subject;

        internal FindProvider()
            : base("find")
        {
            AddParameter(_parameter);

            Description("finds concepts that agent knows");
            Description("find concept according to some constraint");
            Description("give");
            Description("get");
            Description("want");
            Description("lookup");
            Description("look");
            Description("search");
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var criterion = generator.GetValue(instance, _parameter);
            var criterionValues = generator.GetPropertyValues(criterion);
            var requiredProperties = new HashSet<Concept2>(criterionValues.Values.Select(i => i.Concept));
            requiredProperties.Add(criterion.Concept);

            //TODO better matching logic
            var concepts = generator.GetConcepts();
            var result = new List<ConceptInstance>();
            foreach (var concept in concepts)
            {
                var conceptInstance = new ConceptInstance(concept);
                var missingProperties = new HashSet<Concept2>(requiredProperties);

                foreach (var propertyValue in generator.GetPropertyValues(conceptInstance))
                {
                    var property = propertyValue.Key;
                    var value = propertyValue.Value;

                    missingProperties.Remove(value.Concept);
                }

                if (missingProperties.Count == 0)
                {
                    result.Add(conceptInstance);
                }
            }

            if (result.Count == 0)
            {
                generator.Push(new InformationReportEvent(new ConceptInstance(Concept2.NotFound)));
            }
            else if (result.Count == 1)
            {
                generator.Push(new StaticScoreEvent(0.2));
                generator.Push(new InformationReportEvent(result.First()));
            }
            else
            {
                generator.Push(new InformationReportEvent(new ConceptInstance(Concept2.NeedsRefinement)));
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
