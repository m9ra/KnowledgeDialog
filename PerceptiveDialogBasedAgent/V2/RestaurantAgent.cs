using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class RestaurantAgent : EmptyAgent
    {
        private readonly Dictionary<string, string> _specifiers = new Dictionary<string, string>();

        public RestaurantAgent()
            : base()
        {
            Body
                .AddDatabase("restaurant", createRestaurantDatabase())

                .Pattern("i want a $specifier restaurant")
                    .HowToDo("set restaurant specifier $specifier")

                .Pattern("offer restaurant")
                    .HowToDo("say value of name from restaurant database")

                .Pattern("cheap")
                    .WhatItSpecifies("pricerange")
            ;

            AddPolicy("when restaurant database has one result then offer restaurant");
        }

        private DatabaseHandler createRestaurantDatabase()
        {
            var restaurants = new DatabaseHandler();
            restaurants.Columns("pricerange", "name")
                .Row("cheap", "Chinese bistro")
                .Row("expensive", "Ceasar palace")
                ;

            return restaurants;
        }
    }

    static class RestaurantExtensions
    {
        public static readonly string WhatItSpecifiesQ = "what $@ specifies ?";

        public static Body WhatItSpecifies(this Body body, string answerDescription)
        {
            return body.AddPatternFact(WhatItSpecifiesQ, answerDescription);
        }
    }
}
