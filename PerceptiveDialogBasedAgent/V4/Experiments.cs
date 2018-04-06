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
    }
}
