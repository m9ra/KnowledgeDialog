using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Experiments
    {
        public static void Explanation()
        {
            var body = new Body();
            body.Input("say bla bla");
            body.Input("just a saying");
        }

        public static void SimpleRestaurantSearch()
        {
            var body = new Body();
            body.Input("find an expensive restaurant");
        }

        public static void LuxuryRestaurantSearch()
        {
            var body = new Body();
            body.Input("find a luxury restaurant");
            //what is luxury?
            body.Input("it means nice and expensive");
        }

        public static void DialogHandling()
        {
            var body = new Body();
            body.Input("find a restaurant");
            body.Input("luxury one");
            body.Input("nice and expensive");
        }

        public static void RealData()
        {
            var expensiveAnswer = "It means nice and expensive";
            Body body = null;

            body = new Body();
            body.Input("I need discover a restaurant");
            body.Input("to find");
            body.Input("luxury");
            body.Input(expensiveAnswer);

            body = new Body();
            body.Input("look up the location of the four seasons restaurant and send me the address");
            body.Input("its a place where restaurant is located");

            body = new Body();
            body.Input("I would like to find the four seasons restaurant");
            body.Input(expensiveAnswer);

            body = new Body();
            body.Input("list some luxury restaurants in houston");
            body.Input("luxury");
            body.Input(expensiveAnswer);

            body = new Body();
            body.Input("Can you please list some high style restaurants");
            body.Input(expensiveAnswer);

            body = new Body();
            body.Input("Hello, I want to find a famous restaurant");
            body.Input(expensiveAnswer);

            body = new Body();
            body.Input("find the address of a restaurant");
            body.Input("luxury one");
            body.Input(expensiveAnswer);

        }

        public static void HardRealData()
        {
            var expensiveAnswer = "It means nice and expensive";
            var body = new Body();

            body = new Body();
            body.Input("Please send me the address of an Italian restaurant in New York");
        }
    }
}
