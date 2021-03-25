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
            var evt = Get<IncompleteRelationEvent>(s => !s.IsFilled && s.Subject?.Concept == Concept2.PropertyValueDisambiguation, searchInsideTurnOnly: false);
            if (evt == null)
                yield break;

            generator.Push(evt);


            var disambiguation = evt.Subject;
            var unknown = generator.GetValue(disambiguation, Concept2.Unknown);
            var candidates = generator.GetValues(disambiguation, Concept2.Subject);

            var candidateProperties = new HashSet<Concept2>();
            foreach (var candidate in candidates)
            {
                var relevantProperty = generator.GetValue(candidate, Concept2.Property);
                candidateProperties.Add(relevantProperty.Concept);
            }


            if (candidateProperties.Count == 1)
            {
                var candidateString = string.Join(", ", candidates.Select(c => singular(c.Concept)));
                yield return $"I can recognize {candidateString} as {plural(candidateProperties.First())}. Which of them is related to {singular(unknown)}?";
            }
            else if (candidateProperties.Count < 4)
            {
                var candidateString = string.Join(" or ", candidateProperties.Select(c => singular(c)));
                yield return $"What does {singular(unknown)} mean?";
                yield return $"I think, It can be {candidateString}. Which fits best the meaning of {singular(unknown)}?";
            }
            else
            {
                yield return $"What does {singular(unknown)} mean ?";
            }
        }
    }
}
