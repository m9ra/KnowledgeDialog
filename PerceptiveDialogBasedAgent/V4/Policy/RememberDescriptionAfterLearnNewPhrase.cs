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
    class RememberDescriptionAfterLearnNewPhrase : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var askedForLearning = PreviousPolicy<LearnNewPhrase>(out var policyTag) || PreviousPolicy<UnknownAnsweredToLearnNewPhrase>(out policyTag);
            if (!askedForLearning)
                yield break;

            var instances = FindTurnInstances().ToArray();
            if (!instances.Any())
                yield break;

            var unknown = generator.GetValue(policyTag.Tag, Concept2.Unknown);
            var hypothesis = instances.Last();

            generator.Push(new StaticScoreEvent(0.1));
            YesNoPrompt.Generate(generator, RememberConceptDescription.Create(generator, hypothesis.Concept, unknown.Concept.Name), new ConceptInstance(Concept2.Nothing));
            yield return $"So, you think that {singular(unknown)} means {singular(hypothesis)} ?";
        }
    }
}
