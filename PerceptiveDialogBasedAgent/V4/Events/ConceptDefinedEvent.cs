using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class ConceptDefinedEvent : EventBase
    {
        public readonly Concept2 Concept;

        public ConceptDefinedEvent(Concept2 concept)
        {
            Concept = concept;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[new: {Concept.Name}]";
        }
    }
}
