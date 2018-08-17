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
    class ProcessCollectedNewConceptLearning : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Get<InformationReportEvent>(i => i.Instance.Concept == Concept2.LearnNewConcept);
            if (evt == null)
                yield break;

            var collection = evt.Instance;
            var answer = generator.GetValue(collection, Concept2.Answer);
            var unknown = generator.GetValue(collection, Concept2.Unknown);

            var rememberDescription = RememberConceptDescription.Create(generator, answer.Concept, unknown.Concept.Name);
            YesNoPrompt.Generate(generator, rememberDescription, new ConceptInstance(Concept2.Nothing));

            yield return $"So you think {singular(unknown)} is {singular(answer)} ?";
        }
    }
}
