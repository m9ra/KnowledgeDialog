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
        private readonly ParamDefinedEvent _findParam;

        internal RestaurantDomainBeamGenerator()
        {
            var restaurantInstance = DefineConceptInstance("restaurant");
            var pricerange = DefineConcept("pricerange");
            var expensiveInstance = DefineConceptInstance("expensive");
            var cheapInstance = DefineConceptInstance("cheap");

            var restaurant1 = DefineConcept("Ceasar Palace");
            SetProperty(restaurant1, Concept2.InstanceOf, restaurantInstance);
            SetProperty(restaurant1, pricerange, expensiveInstance);

            var restaurant2 = DefineConcept("Chinese Bistro");
            SetProperty(restaurant2, Concept2.InstanceOf, restaurantInstance);
            SetProperty(restaurant2, pricerange, cheapInstance);

            var find = DefineConcept("find");
            var findParameterConstraint = new ConceptInstance(Concept2.Something);
            _findParam = DefineParameter(find, Concept2.Subject, findParameterConstraint);

            AddDescription(find, "finds concepts that agent knows");
            AddDescription(find, "find concept according to some constraint");
            AddDescription(find, "give");
            AddDescription(find, "get");
            AddDescription(find, "want");
            AddDescription(find, "lookup");
            AddDescription(find, "look");
            AddDescription(find, "search");

            AddCallback(find, _find);

            var whatConcept = DefineConcept("what");
            DefineParameter(whatConcept, Concept2.Property, new ConceptInstance(Concept2.Something));
            DefineParameter(whatConcept, Concept2.Subject, new ConceptInstance(Concept2.Something));

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
                Push(new TooManyInstancesFoundEvent(criterion, new SubstitutionRequestEvent(action, _findParam)));
            }
        }
    }
}
