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
        public readonly PropertySetTarget Target;

        public SubstitutionRequestEvent(ConceptInstance targetInstance, ParamDefinedEvent parameterDefinition)
        {
            Target = new PropertySetTarget(targetInstance, parameterDefinition.Property);
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[{Target.Instance.Concept.Name}--{Target.Property.Name}-->?]";
        }
    }
}
