﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class InstanceFoundEvent : EventBase
    {
        public readonly ConceptInstance Instance;

        internal InstanceFoundEvent(ConceptInstance instance)
        {
            Instance = instance;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[found: {Instance.Concept.Name}]";
        }
    }
}
