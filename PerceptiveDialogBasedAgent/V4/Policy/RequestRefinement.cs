using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class RequestRefinement : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Get<InformationReportEvent>();
            var priorityEvt = Get<InformationReportEvent>(p => p.Instance.Concept != Concept2.NeedsRefinement && p.Instance.Concept != Concept2.NotFound);
            if (priorityEvt != null || evt?.Instance.Concept != Concept2.NeedsRefinement)
                yield break;

            var instanceToRefine = generator.GetValue(evt.Instance, Concept2.Subject);

            // let following policies know about the refinement target
            generator.SetValue(TagInstance, Concept2.Target, instanceToRefine);

            // generate a question
            generator.Push(new InformationPartEvent(instanceToRefine, Concept2.Something, null));
            yield return $"I know many {plural(instanceToRefine)} which one would you like?";
        }
    }
}
