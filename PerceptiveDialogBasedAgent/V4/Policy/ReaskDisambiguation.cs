using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class ReaskDisambiguation : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Find<IncompleteRelationEvent>(s => !s.IsFilled && s.Subject?.Concept == Concept2.PropertyValueDisambiguation, precedingTurns: 1);
            if (evt == null)
                yield break;

            var disambiguation = evt.Subject;
            var unknown = generator.GetValue(disambiguation, Concept2.Unknown);

            // retry the event
            generator.Push(evt);
            yield return $"I'm sorry, it did not help me. What does {singular(unknown)} mean?";
            yield return $"I can't see any clues there. What does {singular(unknown)} mean?";
            yield return $"It seems to be complicated. What does {singular(unknown)} mean?";
        }
    }
}
