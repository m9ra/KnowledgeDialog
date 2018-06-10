using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class AskForDisambiguation : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Get<SubstitutionRequestEvent>(s => s.Target?.Instance.Concept == Concept2.PropertyValueDisambiguation);
            if (evt == null)
                yield break;

            var disambiguation = evt.Target.Instance;
            var unknown = generator.GetValue(disambiguation, Concept2.Unknown);
            var candidates = generator.GetValues(disambiguation, Concept2.Subject);

            var candidateProperties = new HashSet<Concept2>();
            foreach (var candidate in candidates)
            {
                var relevantProperty = generator.GetValue(candidate, Concept2.Property);
                candidateProperties.Add(relevantProperty.Concept);
            }

            generator.Push(evt);

            if (candidateProperties.Count > 1)
            {
                yield return $"What does {singular(unknown)} mean ?";
            }
            else
            {
                var candidateString = string.Join(", ", candidates.Select(c => singular(c.Concept)));
                yield return $"I can recognize {candidateString} as {plural(candidateProperties.First())}. Which of them is related to {singular(unknown)}?";
            }
        }
    }
}
