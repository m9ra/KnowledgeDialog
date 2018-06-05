using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class OfferResult : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Get<InformationReportEvent>();
            if (evt == null)
                yield break;

            generator.Push(new InstanceReferencedEvent(evt.Instance));

            yield return $"I think you would like {singular(evt.Instance)}";
            yield return $"I know {singular(evt.Instance)}";
        }
    }
}
