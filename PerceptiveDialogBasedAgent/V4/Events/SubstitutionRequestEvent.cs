using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class SubstitutionRequestEvent_old : EventBase
    {
        public readonly PropertySetTarget Target;

        public readonly ConceptInstance ActivationTarget;

        public SubstitutionRequestEvent_old(ConceptInstance targetInstance, ParamDefinedEvent parameterDefinition)
        {
            Target = new PropertySetTarget(targetInstance, parameterDefinition.Property);
        }

        public SubstitutionRequestEvent_old(PropertySetTarget target, ConceptInstance activationTarget = null)
        {
            Target = target;
            ActivationTarget = activationTarget;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            var requestRepresentation = $"[{Target.Instance.Concept.Name}--{Target.Property.Name}-->?]";
            if (ActivationTarget != null)
                requestRepresentation += " | " + ActivationTarget.ToPrintable() + " ";

            return requestRepresentation;
        }
    }
}
