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
            Description("tell");
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
            criterionValues.Remove(Concept2.OnSetListener); // TODO internal property removal should be done in more systematic way

            var requiredProperties = new HashSet<Concept2>(criterionValues.Values.Select(i => i.Concept));
            requiredProperties.Add(criterion.Concept);

            var result = FindRelevantConcepts(generator, requiredProperties);

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
                var needRefinementInstance = new ConceptInstance(Concept2.NeedsRefinement);
                generator.SetValue(needRefinementInstance, Concept2.Subject, criterion);
                generator.SetValue(criterion, Concept2.OnSetListener, instance);
                generator.Push(new InformationReportEvent(needRefinementInstance));
            }
        }

        internal static List<ConceptInstance> FindRelevantConcepts(BeamGenerator generator, HashSet<Concept2> requiredProperties)
        {
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

            return result;
        }
    }
}
