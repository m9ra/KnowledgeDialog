using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class RestaurantDomainKnowledge : KnowledgeAbilityBase
    {
        private readonly Concept2 _pricerangeProperty = Concept2.From("pricerange");
        private readonly Concept2 _nameProperty = Concept2.From("name");
        private readonly Concept2 _restaurantClass = Concept2.From("restaurant");

        public RestaurantDomainKnowledge()
        {
            DefineConcept("name", out _nameProperty);
            DefineConcept("pricerange", out _pricerangeProperty)
                .Description("price")
                //.Description("prices")
                .Description("money")
                .Description("pricy");

            DefineConcept("restaurant", out _restaurantClass)
                .Description("venue to eat");

            DefineConcept("food", out var foodConcept)
                .Description("stuff to eat");

            DefineConcept("pizza", out var pizzaConcept)
                .Property(Concept2.InstanceOf, foodConcept);

            DefineConcept("cheap", out var cheapConcept)
                .Property(Concept2.InstanceOf, _pricerangeProperty);
            DefineConcept("expensive", out var expensiveConcept)
                .Property(Concept2.InstanceOf, _pricerangeProperty);
            DefineConcept("moderate", out var moderateConcept)
                .Property(Concept2.InstanceOf, _pricerangeProperty);

            SetValues(_pricerangeProperty, Concept2.HasPropertyValue, expensiveConcept, moderateConcept, cheapConcept);
            SetValues(_restaurantClass, Concept2.HasProperty, _pricerangeProperty);

            DefineRestaurant("Ceasar Palace", expensiveConcept);
            DefineRestaurant("Bombay", null);
            DefineRestaurant("Vapiano", moderateConcept);
            DefineRestaurant("Chinese Bistro", cheapConcept);
        }

        internal void DefineRestaurant(string name, Concept2 pricerange, params string[] descriptions)
        {
            DefineConcept(name, out var restaurantConcept)
                .Property(Concept2.InstanceOf, _restaurantClass)
                //.Property(_nameProperty, restaurantConcept)
                ;

            if (pricerange != null)
                Property(_pricerangeProperty, pricerange);

            foreach (var description in descriptions)
            {
                Description(description);
            }
        }
    }
}
