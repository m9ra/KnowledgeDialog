﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class FrameEvent : EventBase
    {
        internal readonly ConceptInstance Goal;

        internal FrameEvent(ConceptInstance goal)
        {
            Goal = goal;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[goal: {Goal.ToPrintable()}]";
        }
    }
}
