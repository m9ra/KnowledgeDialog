﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class InstanceOutputEvent : EventBase
    {
        public readonly ConceptInstance Instance;

        internal InstanceOutputEvent(ConceptInstance instance)
        {
            Instance = instance;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[info: {Instance.Concept.Name}]";
        }
    }
}
