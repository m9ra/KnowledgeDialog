using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class InstanceActiveEvent : EventBase
    {
        internal readonly ConceptInstance Instance;

        internal readonly InstanceActivationRequestEvent Request;

        internal InstanceActiveEvent(ConceptInstance instance, InstanceActivationRequestEvent request = null)
        {
            Instance = instance;
            Request = request;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[active: {Instance.Concept.Name}]";
        }
    }
}
