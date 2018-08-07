using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class PolicyTagEvent : EventBase
    {
        internal readonly ConceptInstance Tag;

        internal PolicyTagEvent(ConceptInstance tag)
        {
            Tag = tag;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[tag: {Tag.Concept.Name}]";
        }
    }
}
