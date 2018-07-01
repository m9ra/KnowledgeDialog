using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class ExportEvent : EventBase
    {
        internal readonly EventBase ExportedEvent;

        internal ExportEvent(EventBase exportedEvent)
        {
            ExportedEvent = exportedEvent;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"export: {ExportedEvent}";
        }
    }
}
