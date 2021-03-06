﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class RestaurantAgent : EmptyAgent
    {
        private readonly DatabaseHandler _db;

        public RestaurantAgent()
            : base()
        {
            Body.AddDatabase("restaurant", _db = CreateRestaurantDatabase());
            Body.Db.Container
                .Pattern("hello", "hi")
                    .HowToEvaluate("greeting")

                .Pattern(
                    "give me $specifier restaurant",
                    "i want $specifier restaurant",
                    "i want a $specifier restaurant",
                    "i want an $specifier restaurant"
                    )
                    .HowToDo("set restaurant specifier $specifier")

                .Pattern("offer the restaurant")
                    .HowToDo("say I know a restaurant joined with value of name from restaurant database")
            ;

            AddPolicy("when restaurant database was updated and restaurant database has one result then offer the restaurant");
            AddPolicy("when user said greeting then ask how can i help you");
            AddPolicy("when user said i want a restaurant then ask which one");
        }

        internal string RestaurantSpecifier(string slotName)
        {
            var value = _db.GetSpecifier(slotName);
            return value;
        }

        internal static DatabaseHandler CreateRestaurantDatabase()
        {
            /*var restaurants = LoadDstcRestaurants("restaurants.db.json");
            return restaurants;*/

            var restaurants = new DatabaseHandler();
            restaurants.SetColumns("pricerange", "name")
                .Row("cheap", "Chinese bistro")
                .Row("expensive", "Ceasar palace")
                ;

            return restaurants;
        }

        internal static DatabaseHandler LoadDstcRestaurants(string dbFile)
        {
            var restaurants = new DatabaseHandler();
            var json = File.ReadAllText(dbFile);
            var entries = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<RestaurantEntry[]>(json);

            restaurants.SetColumns("pricerange", "name");
            foreach (var entry in entries)
            {
                restaurants.Row(entry.pricerange, entry.name);
            }
            return restaurants;
        }
    }

    [Serializable]
    class RestaurantEntry
    {
        /*{
        "phone": "01223 461661",
        "pricerange": "expensive",
        "addr": "31 newnham road newnham",
        "area": "west",
        "food": "indian",
        "postcode": "not available",
        "name": "india house"
        */

        public readonly string phone;

        public readonly string pricerange;

        public readonly string addr;

        public readonly string area;

        public readonly string food;

        public readonly string postcode;

        public readonly string name;
    }

    static class RestaurantExtensions
    {
        public static DataContainer WhatItSpecifies(this DataContainer container, string answerDescription)
        {
            return container.AddPatternFact(Question.WhatItSpecifies, answerDescription);
        }
    }
}
