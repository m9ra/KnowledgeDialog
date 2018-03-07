using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    class Experiments
    {
        public static void Explanation()
        {
            var agent = new PointingAgent();

            agent.Input("say current time");
            //what does current time mean?
            agent.Input("it means time on the clock right now");
        }
    }
}
