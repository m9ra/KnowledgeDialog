﻿using System;
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
            if (evt?.Instance.Concept != Concept2.NeedsRefinement)
                yield break;

            var instanceToRefine = generator.GetValue(evt.Instance, Concept2.Subject);

            var target = new PropertySetTarget(instanceToRefine, Concept2.Something);
            generator.Push(new SubstitutionRequestEvent(target));

            yield return $"I know many {plural(instanceToRefine)} which one would you like?";
        }
    }
}