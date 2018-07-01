using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class OfferResult : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Get<InformationReportEvent>();
            if (evt == null)
                yield break;

            var concept = evt.Instance.Concept;

            if (concept == Concept2.NotFound)
            {
                yield return $"I don't know anything like that.";
            }
            else if (concept == Concept2.DisambiguationFailed)
            {
                yield return $"It is too complex for me. Could you use different words?";
            }
            else if (concept == Concept2.RememberPropertyValue)
            {
                yield return $"Ok, I'll remember that.";
                yield return $"Thank you for the information!";
            }
            else if (concept == Concept2.KnowledgeConfirmed)
            {
                var information = generator.GetValue(evt.Instance, Concept2.Subject);
                generator.Push(new InstanceActiveEvent(information, canBeReferenced: true));
                yield return $"Yes, I know {singularWithProperty(information)}";
            }
            else if (concept == Concept2.KnowledgeRefutation)
            {
                var information = generator.GetValue(evt.Instance, Concept2.Subject);
                generator.Push(new InstanceActiveEvent(information, canBeReferenced: true));
                yield return $"No, I don't know {singularWithProperty(information)}";
            }
            else
            {
                generator.Push(new InstanceActiveEvent(evt.Instance, canBeReferenced: true));

                yield return $"I think you would like {singular(evt.Instance)}";
                yield return $"I know {singular(evt.Instance)}";
            }
        }
    }
}
