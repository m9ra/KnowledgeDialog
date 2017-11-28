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
            var agent = new RestaurantAgent();
            agent.Input("i want a cheap restaurant");
        }

        internal static void RestaurantSearchLearningTest()
        {
            var agent = new RestaurantAgent();

            agent.Input("i want a luxury restaurant");
            //what does luxury specify?
            agent.Input("pricerange");
            //it means cheap?
            agent.Input("it means expensive");
            //ok, so i can offer Ceasar Palace Restaurant to you
        }
    }
}
