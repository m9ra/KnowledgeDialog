using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class ConceptDescriptionEvent : EventBase
    {
        public readonly Concept2 Concept;

        public readonly string Description;

        public ConceptDescriptionEvent(Concept2 concept, string description)
        {
            Concept = concept;
            Description = description;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }
    }
}
