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
            Body.AddDatabase("restaurant", CreateRestaurantDatabase());
            Body.Db.Container
                .Pattern("i want a $specifier restaurant")
                    .HowToDo("set restaurant specifier $specifier")

                .Pattern("offer the restaurant")
                    .HowToDo("say there is a restaurant joined with value of name from restaurant database")

                .Pattern("cheap")
                    .WhatItSpecifies("pricerange")
            ;
                        
            AddPolicy("when restaurant database was updated and restaurant database has one result then offer the restaurant");
        }

        internal static DatabaseHandler CreateRestaurantDatabase()
        {
            var restaurants = new DatabaseHandler();
            restaurants.SetColumns("pricerange", "name")
                .Row("cheap", "Chinese bistro")
                .Row("expensive", "Ceasar palace")
                ;

            return restaurants;
        }
    }

    static class RestaurantExtensions
    {
        public static DataContainer WhatItSpecifies(this DataContainer container, string answerDescription)
        {
            return container.AddPatternFact(Question.WhatItSpecifies, answerDescription);
        }
    }
}
