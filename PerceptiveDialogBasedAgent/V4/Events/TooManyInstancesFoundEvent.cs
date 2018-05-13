using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class TooManyInstancesFoundEvent : EventBase
    {
        internal readonly ConceptInstance Criterion;

        public TooManyInstancesFoundEvent(ConceptInstance criterion)
        {
            Criterion = criterion;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }
    }
}
