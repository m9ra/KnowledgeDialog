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

            if (evt.Instance.Concept == Concept2.NotFound)
            {
                yield return $"I don't know anything like that.";
            }
            else if (evt.Instance.Concept == Concept2.DisambiguationFailed)
            {
                yield return $"It is too complex for me. Could you use different words?";
            }
            else
            {
                generator.Push(new InstanceActiveEvent(evt.Instance));

                yield return $"I think you would like {singular(evt.Instance)}";
                yield return $"I know {singular(evt.Instance)}";
            }
        }
    }
}
