using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Abilities;
using PerceptiveDialogBasedAgent.V4.EventBeam;

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
            if (instances.Count() != 1)
                yield break;

            var unknown = generator.GetValue(policyTag.Tag, Concept2.Unknown);
            var hypothesis = instances.First();

            YesNoPrompt.Generate(generator, RememberConceptDescription.Create(generator, hypothesis.Concept, unknown.Concept.Name), new ConceptInstance(Concept2.Nothing));
            yield return $"So, you think that {singular(unknown)} is {singular(hypothesis)} ?";
        }
    }
}
