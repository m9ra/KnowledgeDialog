using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class AfterPropertyLearned : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Get<PropertySetEvent>(p => p.Target.Property == Concept2.HasProperty);
            if (evt == null)
                yield break;

            var learnedProperty = evt.SubstitutedValue;
            var propertyTarget = evt.Target.Instance;
            var request = new InformationPartEvent(propertyTarget, learnedProperty.Concept, null);

            generator.Push(request);
            yield return $"What value of '{singular(learnedProperty)}' does {singular(propertyTarget)} have ?";
        }
    }
}
