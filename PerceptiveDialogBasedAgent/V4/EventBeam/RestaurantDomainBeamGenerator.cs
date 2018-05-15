using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class RestaurantDomainBeamGenerator : PolicyBeamGenerator
    {
        public static readonly Concept2 Find = new Concept2("find");

        public readonly ParamDefinedEvent FindParam;


        internal RestaurantDomainBeamGenerator()
        {
            var restaurant = new Concept2("restaurant");
            var restaurantInstance = new ConceptInstance(restaurant);
            PushToAll(new ConceptDefinedEvent(restaurant));

            var pricerange = new Concept2("pricerange");
            PushToAll(new ConceptDefinedEvent(pricerange));

            var expensive = new Concept2("expensive");
            var expensiveInstance = new ConceptInstance(expensive);
            PushToAll(new ConceptDefinedEvent(expensive));

            var cheap = new Concept2("cheap");
            var cheapInstance = new ConceptInstance(cheap);
            PushToAll(new ConceptDefinedEvent(cheap));

            var restaurant1 = new Concept2("Ceasar Palace");
            PushToAll(new ConceptDefinedEvent(restaurant1));
            SetProperty(restaurant1, Concept2.InstanceOf, restaurantInstance);
            SetProperty(restaurant1, pricerange, expensiveInstance);

            var restaurant2 = new Concept2("Chinese Bistro");
            PushToAll(new ConceptDefinedEvent(restaurant2));
            SetProperty(restaurant2, Concept2.InstanceOf, restaurantInstance);
            SetProperty(restaurant2, pricerange, cheapInstance);

            var findParameterConstraint = new ConceptInstance(Concept2.Something);

            PushToAll(new ConceptDefinedEvent(Find));
            PushToAll(FindParam = new ParamDefinedEvent(Find, Concept2.Subject, findParameterConstraint));
            PushToAll(new ConceptDescriptionEvent(Find, "finds concepts that agent knows"));
            PushToAll(new ConceptDescriptionEvent(Find, "find concept according to some constraint"));
            PushToAll(new ConceptDescriptionEvent(Find, "give"));
            PushToAll(new ConceptDescriptionEvent(Find, "get"));
            PushToAll(new ConceptDescriptionEvent(Find, "want"));
            PushToAll(new ConceptDescriptionEvent(Find, "lookup"));
            PushToAll(new ConceptDescriptionEvent(Find, "look"));
            PushToAll(new ConceptDescriptionEvent(Find, "search"));

            AddCallback(Find, _find);

            var whatConcept = new Concept2("what");
            PushToAll(new ConceptDefinedEvent(whatConcept));
            PushToAll(new ParamDefinedEvent(whatConcept, Concept2.Property, new ConceptInstance(Concept2.Something)));
            PushToAll(new ParamDefinedEvent(whatConcept, Concept2.Subject, new ConceptInstance(Concept2.Something)));

            AddCallback(whatConcept, _what);

        }

        private void _what(ConceptInstance action, ExecutionBeamGenerator generator)
        {
            var property = GetValue(action, Concept2.Property);
            var subject = GetValue(action, Concept2.Subject);

            var value = GetValue(subject, property.Concept);
            if (value == null)
            {
                Push(new NoInstanceFoundEvent(property));
            }
            else
            {
                Push(new StaticScoreEvent(0.20));
                Push(new InstanceFoundEvent(value));
            }
        }

        private void _find(ConceptInstance action, ExecutionBeamGenerator generator)
        {
            //TODO we need more complex patterns here
            var criterion = GetValue(action, Concept2.Subject);
            var criterionConcept = criterion.Concept;
            var concepts = GetConcepts();

            var result = new List<ConceptInstance>();
            foreach (var concept in concepts)
            {
                var conceptInstance = new ConceptInstance(concept);
                foreach (var propertyValue in GetPropertyValues(conceptInstance))
                {
                    var property = propertyValue.Key;
                    var value = propertyValue.Value;

                    if (property == criterionConcept || value?.Concept == criterionConcept)
                    {
                        result.Add(conceptInstance);
                        break;
                    }
                }
            }

            if (result.Count == 0)
            {
                Push(new NoInstanceFoundEvent(criterion));
            }
            else if (result.Count == 1)
            {
                Push(new StaticScoreEvent(0.2));
                Push(new InstanceFoundEvent(result.First()));
            }
            else
            {
                Push(new TooManyInstancesFoundEvent(criterion, new SubstitutionRequestEvent(action, FindParam)));
            }
        }
    }
}
