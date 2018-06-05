using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class RestaurantDomainKnowledge : KnowledgeAbilityBase
    {
        public RestaurantDomainKnowledge()
        {
            DefineConcept("restaurant", out var restaurantConcept)
                .Description("venue to eat");

            DefineConcept("food", out var foodConcept)
                .Description("stuff to eat");

            DefineConcept("pizza", out var pizzaConcept)
                .Property(Concept2.InstanceOf, foodConcept);

            DefineConcept("pricerange", out var pricerangeConcept);
            DefineConcept("name", out var nameConcept);
            DefineConcept("cheap", out var cheapConcept)
                .Property(Concept2.InstanceOf, pricerangeConcept);
            DefineConcept("expensive", out var expensiveConcept)
                .Property(Concept2.InstanceOf, pricerangeConcept);

            DefineConcept("Ceasar Palace", out var ceasarPalace)
                .Property(Concept2.InstanceOf, restaurantConcept)
                .Property(pricerangeConcept, expensiveConcept)
                .Property(nameConcept, ceasarPalace);

            DefineConcept("Chinese Palace", out var chinesePalace)
                .Property(Concept2.InstanceOf, restaurantConcept)
                .Property(pricerangeConcept, cheapConcept)
                .Property(nameConcept, chinesePalace);
        }
    }
}
