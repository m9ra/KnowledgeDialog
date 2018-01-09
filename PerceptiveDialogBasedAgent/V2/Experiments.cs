using PerceptiveDialogBasedAgent.V2.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    static class Experiments
    {
        internal static void EmptyAgentTest()
        {
            // Database.DebugTrigger(32);

            var agent = new EmptyAgent();

            agent.Input("say what is instead of how to evaluate");
            agent.Input("hello");
            agent.Input("say hi");
            agent.Input("say I understand instead of ok");
        }

        internal static void NewInfoTest()
        {
            var agent = new EmptyAgent();

            agent.Input("say what is instead of how to evaluate");
            agent.Input("hello");
            agent.Input("hello is a greeting");
        }

        internal static void RestaurantSearchTest()
        {
            //Database.DebugTrigger(229);
            var agent = new RestaurantAgent();

            agent.Input("i want a luxury restaurant");
        }

        internal static void RestaurantSearchLearningTest()
        {
            //Database.DebugTrigger(693);
            var agent = new RestaurantAgent();

            agent.Input("i want a luxury restaurant");
            //what does luxury specify?
            agent.Input("pricerange");

            
            agent.Input("i want a luxury restaurant"); //DEBUG ONLY
            //how to paraphrase luxury ?
            agent.Input("expensive");

            agent.Input("i want a luxury restaurant"); //DEBUG ONLY
        }

        internal static void ModuleTesting()
        {
            var externalDatabase = RestaurantAgent.CreateRestaurantDatabase();
            var module = new ExternalDatabaseProviderModule("restaurant", externalDatabase);

            var database = new EvaluatedDatabase();
            database.Container
                .Pattern("luxury")
                    .WhatItSpecifies("pricerange")
                    .HowToEvaluate("expensive")
            ;

            database.StartQueryLog();
            module.AttachTo(database);

            var result = database.Query(SemanticItem.AnswerQuery(Question.HowToDo, Constraints.WithInput("set restaurant specifier luxury")));
            //var result = database.Query(SemanticItem.AnswerQuery(Question.IsItTrue, Constraints.WithInput("restaurant database has 1 result")));

            var actionId = result.FirstOrDefault().Answer;
            var action = database.GetNativeAction(actionId);

            action(result.FirstOrDefault());

            var log = database.FinishLog();
            Log.Questions(log.GetQuestions());
            Log.Result(result);

            Log.Dump(database);
        }
    }
}
