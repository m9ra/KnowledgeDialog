using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class CompleteInstanceEvent : EventBase
    {
        internal readonly InstanceActivationEvent InstanceActivation;

        internal ConceptInstance Instance => InstanceActivation.Instance;

        internal CompleteInstanceEvent(InstanceActivationEvent instanceActivation)
        {
            InstanceActivation = instanceActivation;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[complete: {Instance.Concept.Name}]";
        }
    }
}
