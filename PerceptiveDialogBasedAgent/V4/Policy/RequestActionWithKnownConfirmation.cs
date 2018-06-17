using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class RequestActionWithKnownConfirmation : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var activeInstance = Get<InstanceActiveEvent>(e => e.Request != null);
            if (activeInstance == null)
                yield break;

            generator.Push(new InstanceReferencedEvent(activeInstance.Instance));
            yield return $"I know {singular(activeInstance.Instance)} but what should I do?";
        }
    }
}
