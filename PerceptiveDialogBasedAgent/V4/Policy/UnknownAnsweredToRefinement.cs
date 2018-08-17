using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Abilities;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class UnknownAnsweredToRefinement : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var unknownPhrases = GetUnknownPhrases(generator);
            if (!PreviousPolicy<RequestRefinement>(out var refinementPolicy) || unknownPhrases.Count() != 1)
                yield break;

            var instanceToRefine = generator.GetValue(refinementPolicy.Tag, Concept2.Target);

            var unknown = unknownPhrases.First();
            var assignUnknownProperty = AssignUnknownProperty.Create(instanceToRefine, unknown, generator);

            YesNoPrompt.Generate(generator, assignUnknownProperty, instanceToRefine);
            yield return $"I suppose, you would like to find {plural(instanceToRefine)} which are {unknown}?";
        }
    }
}
