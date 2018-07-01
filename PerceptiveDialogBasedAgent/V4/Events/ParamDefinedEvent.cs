using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class ParamDefinedEvent : EventBase
    {
        internal readonly Concept2 Concept;
        internal readonly Concept2 Property;
        internal readonly ConceptInstance ValueConstraint;

        public ParamDefinedEvent(Concept2 concept, Concept2 property, ConceptInstance valueConstraint)
        {
            Concept = concept;
            Property = property;
            ValueConstraint = valueConstraint;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[param {Concept.Name}--{Property.Name}-->{ValueConstraint.Concept.Name}]";
        }
    }
}
