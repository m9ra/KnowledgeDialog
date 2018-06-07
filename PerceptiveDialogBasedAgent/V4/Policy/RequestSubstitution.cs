using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class RequestSubstitution : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var request = Get<SubstitutionRequestEvent>();
            if (request == null)
                yield break;

            generator.Push(request);
            yield return "What should I " + singular(request.Target.Instance) + " ?";
        }
    }
}
