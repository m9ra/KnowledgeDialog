using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class AcceptNewProperty : ConceptAbilityBase
    {
        internal AcceptNewProperty() : base(Concept2.AcceptNewProperty)
        {
            AddParameter(Concept2.Property);
            AddParameter(Concept2.Target);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var property = generator.GetValue(instance, Concept2.Property);
            var target = generator.GetValue(instance, Concept2.Target);

            var setTarget = new PropertySetTarget(target, Concept2.HasProperty);
            var evt = new PropertySetEvent(setTarget, property, allowActivation: false);

            generator.Push(evt);
            generator.Push(new ExportEvent(evt));
        }
    }
}
