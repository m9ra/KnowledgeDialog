﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class CloseEvent : EventBase
    {
        internal readonly EventBase ClosedEvent;

        internal CloseEvent(EventBase closedEvent)
        {
            closedEvent = ClosedEvent;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return "!" + ClosedEvent.ToString();
        }
    }
}
