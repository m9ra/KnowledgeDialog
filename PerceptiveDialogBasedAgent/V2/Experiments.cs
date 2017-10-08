using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    static class Experiments
    {
        internal static void EmptyAgentTests()
        {
            var agent = new EmptyAgent();

            //agent.Input("hello");
            agent.Input("say hi");
        }
    }
}
