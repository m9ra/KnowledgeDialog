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
            Description("need");
            Description("want");
            Description("lookup");
            Description("list");
            Description("show");
            Description("name a");
            Description("listing");
            Description("look up");
            Description("look");
            Description("look for");
            Description("looking for");
            Description("looking");
            Description("searching");
            Description("searching for");
            Description("search");
            Description("what");
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var criterion = generator.GetValue(instance, _parameter);
            var criterionValues = generator.GetPropertyValues(criterion);
            criterionValues.Remove(Concept2.OnSetListener); // TODO internal property removal should be done in more systematic way
            criterionValues.Remove(Concept2.HasProperty);
            criterionValues.Remove(Concept2.HasPropertyValue);

            var requiredProperties = new HashSet<Concept2>(criterionValues.Values.Select(i => i.Concept));
            requiredProperties.Add(criterion.Concept);

            var result = FindRelevantConcepts(generator, requiredProperties);
            var isSubjectClass = generator.GetInverseConceptValues(Concept2.InstanceOf, criterion).Any();


            if (result.Count == 0)
            {
                if (isSubjectClass)
                    generator.Push(new StaticScoreEvent(0.1));
                else
                    generator.Push(new StaticScoreEvent(-0.1));

                generator.Push(new InformationReportEvent(new ConceptInstance(Concept2.NotFound)));
            }
            else if (result.Count == 1)
            {
                generator.Push(new StaticScoreEvent(0.2));
                generator.Push(new InformationReportEvent(result.First()));
            }
            else
            {
                if (generator.IsProperty(criterion.Concept))
                    generator.Push(new StaticScoreEvent(-0.15));

                var needRefinementInstance = new ConceptInstance(Concept2.NeedsRefinement);
                generator.SetValue(needRefinementInstance, Concept2.Subject, criterion);
                generator.SetValue(criterion, Concept2.OnSetListener, instance);
                generator.Push(new InformationReportEvent(needRefinementInstance));
            }
        }

        internal static List<ConceptInstance> FindRelevantConcepts(BeamGenerator generator, HashSet<Concept2> requiredProperties)
        {
            //TODO better matching logic
            var concepts = generator.GetDefinedConcepts();
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
