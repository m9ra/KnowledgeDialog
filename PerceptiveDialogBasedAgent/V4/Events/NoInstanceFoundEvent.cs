using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class NoInstanceFoundEvent : EventBase
    {
        private Concept2 criterion;

        public NoInstanceFoundEvent(Concept2 criterion)
        {
            this.criterion = criterion;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }
    }
}
