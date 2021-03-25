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
            var suppressedConcepts = new HashSet<Concept2>
            {
                 Concept2.NotFound,
                 Concept2.DisambiguatedKnowledgeConfirmed,
                 Concept2.NeedsRefinement
            };

            var evt = Get<InstanceOutputEvent>();
            var priorityEvt = Get<InstanceOutputEvent>(p => !suppressedConcepts.Contains(p.Instance.Concept));
            if (priorityEvt != null)
                evt = priorityEvt;

            if (evt == null)
                yield break;

            var concept = evt.Instance.Concept;
            generator.SetValue(TagInstance, Concept2.Subject, evt.Instance);

            if (concept == Concept2.DisambiguatedKnowledgeConfirmed)
            {
                var disamb = Find<IncompleteRelationEvent>(s => !s.IsFilled && s.Subject?.Concept == Concept2.PropertyValueDisambiguation, precedingTurns: 1);
                var confirmed = generator.GetValue(evt.Instance, Concept2.Subject);
                var disambiguator = generator.GetValue(evt.Instance, Concept2.Target);
                generator.Push(new IncompleteRelationEvent(disambiguator, Concept2.Answer, null));

                yield return $"I know it is {singular(confirmed)} already. Could you be more specific?";
            }
            else if (concept == Concept2.NotFound)
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
                var property = generator.GetValue(evt.Instance, Concept2.Property).Concept;
                generator.Push(new InstanceActiveEvent(information, canBeReferenced: true));
                generator.Push(new IncompleteRelationEvent(information, property, null));
                yield return $"No, I don't know {singularWithProperty(information)}";
            }
            else
            {
                generator.Push(new InstanceActiveEvent(evt.Instance, canBeReferenced: true));

                yield return $"It is {singular(evt.Instance)}";
                yield return $"I found {singular(evt.Instance)}";
            }
        }
    }
}
