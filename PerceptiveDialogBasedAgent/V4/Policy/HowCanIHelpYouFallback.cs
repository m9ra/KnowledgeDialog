using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class HowCanIHelpYouFallback : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            yield return "How can I help you?";
            yield return "I need to know what should I do.";
            yield return "Please, tell me what should I do.";
        }
    }
}
