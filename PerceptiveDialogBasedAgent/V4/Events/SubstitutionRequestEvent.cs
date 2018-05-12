using PerceptiveDialogBasedAgent.V4.Brain;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class SubstitutionRequestEvent : EventBase
    {
        public readonly ConceptInstance TargetInstance;

        public SubstitutionRequestEvent(ConceptInstance targetInstance, TargetDefinedEvent parameterDefinition)
        {
            TargetInstance = targetInstance;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }
    }
}
