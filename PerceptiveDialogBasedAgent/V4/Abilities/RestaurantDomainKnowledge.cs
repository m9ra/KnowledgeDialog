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
        private readonly Concept2 _addressProperty = Concept2.From("address");
        private readonly Concept2 _nameProperty = Concept2.From("name");
        private readonly Concept2 _restaurantClass = Concept2.From("restaurant");

        public RestaurantDomainKnowledge()
        {
            DefineConcept("name", out _nameProperty);
            DefineConcept("address", out _addressProperty);
            DefineConcept("pricerange", out _pricerangeProperty)
                .Description("price")
                //.Description("prices")
                .Description("money")
                .Description("pricy");

            DefineConcept("restaurant", out _restaurantClass)
                .Description("restaurants")
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

            DefineRestaurant("Caesar Palace", expensiveConcept, Concept2.From("Balleary Street"));
            DefineRestaurant("Bombay", null, Concept2.From("V Parku"));
            DefineRestaurant("Vapiano", moderateConcept, Concept2.From("Chodov Avenue"));
            DefineRestaurant("Chinese Bistro", cheapConcept, Concept2.From("Montreal"));
        }

        internal void DefineRestaurant(string name, Concept2 pricerange, Concept2 address, params string[] descriptions)
        {
            DefineConcept(name, out var restaurantConcept)
                .Property(Concept2.InstanceOf, _restaurantClass)
                //.Property(_nameProperty, restaurantConcept)
                ;

            if (pricerange != null)
                Property(_pricerangeProperty, pricerange);

            Property(_addressProperty, address);

            foreach (var description in descriptions)
            {
                Description(description);
            }

            DefineConcept(address);
            SetValues(address, Concept2.InstanceOf, _addressProperty);
            SetValues(_addressProperty, Concept2.HasPropertyValue, address);
        }
    }
}
