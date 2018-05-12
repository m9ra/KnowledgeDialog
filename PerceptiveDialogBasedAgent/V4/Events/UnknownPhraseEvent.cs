using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class UnknownPhraseEvent : EventBase
    {
        private InputPhraseEvent evt;

        public UnknownPhraseEvent(InputPhraseEvent evt)
        {
            this.evt = evt;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }
    }
}
