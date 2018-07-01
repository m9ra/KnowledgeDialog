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
    class RememberPropertyValue : ConceptAbilityBase
    {
        internal RememberPropertyValue() : base(Concept2.RememberPropertyValue.Name)
        {
            AddParameter(Concept2.Target);
            AddParameter(Concept2.TargetProperty);
            AddParameter(Concept2.Subject);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var target = generator.GetValue(instance, Concept2.Target);
            var targetProperty = generator.GetValue(instance, Concept2.TargetProperty);
            var subject = generator.GetValue(instance, Concept2.Subject);

            var setTarget = new PropertySetTarget(target, targetProperty.Concept);
            generator.Push(new ExportEvent(new PropertySetEvent(setTarget, subject)));
            generator.Push(new InformationReportEvent(instance));
        }
    }
}
