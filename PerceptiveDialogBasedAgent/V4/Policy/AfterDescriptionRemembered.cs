using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class AfterDescriptionRemembered : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            //we are searching for deactivated instances because after call instance is deactivated
            var rememberAbility = FindDeactivatedTurnInstances(i => i.Concept == Concept2.RememberConceptDescription).FirstOrDefault();
            if (rememberAbility == null)
                yield break;

            var subject = generator.GetValue(rememberAbility, Concept2.Subject);

            generator.Push(new Events.InstanceReferencedEvent(subject));
            yield return $"Thank you. What should I do with {singular(subject)}?";
        }
    }
}
