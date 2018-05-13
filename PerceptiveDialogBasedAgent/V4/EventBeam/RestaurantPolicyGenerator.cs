using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class RestaurantPolicyGenerator : ExecutionBeamGenerator
    {
        public static readonly Concept2 Find = new Concept2("find");


        internal RestaurantPolicyGenerator()
        {
            var restaurant = new Concept2("restaurant");
            var restaurantInstance = new ConceptInstance(restaurant);
            PushToAll(new ConceptDefinedEvent(restaurant));

            var pricerange = new Concept2("pricerange");

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
            PushToAll(new ParamDefinedEvent(Find, Concept2.Subject, findParameterConstraint));
            PushToAll(new ConceptDescriptionEvent(Find, "finds concepts that agent knows"));
            PushToAll(new ConceptDescriptionEvent(Find, "find concept according to some constraint"));

            AddCallback(Find, _find);
        }

        private void _find(ConceptInstance action, ExecutionBeamGenerator generator)
        {
            //TODO we need more complex patterns here
            var criterion = GetValue(action, Concept2.Subject).Concept;
            var concepts = GetConcepts();

            var result = new List<ConceptInstance>();
            foreach (var concept in concepts)
            {
                var conceptInstance = new ConceptInstance(concept);
                foreach (var propertyValue in GetPropertyValues(conceptInstance))
                {
                    var property = propertyValue.Key;
                    var value = propertyValue.Value;

                    if (property == criterion || value?.Concept == criterion)
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
                Push(new InstanceFoundEvent(result.First()));
                Push(new StaticScoreEvent(0.2));
            }
            else
            {
                Push(new TooManyInstancesFoundEvent(criterion));
            }
        }
    }
}
