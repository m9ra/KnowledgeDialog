using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class PropertySetEvent : EventBase
    {
        internal readonly PropertySetTarget Target;

        internal readonly ConceptInstance SubstitutedValue;

        internal readonly bool AllowActivation;

        public PropertySetEvent(PropertySetTarget target, ConceptInstance value, bool allowActivation = true)
        {
            Target = target;
            SubstitutedValue = value;
            AllowActivation = allowActivation;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            var targetDescriptor = Target.Concept?.Name ?? Target.Instance.Concept.Name;
            return $"[{targetDescriptor}<--{Target.Property.Name}--{SubstitutedValue.Concept.Name}]";
        }
    }
}
