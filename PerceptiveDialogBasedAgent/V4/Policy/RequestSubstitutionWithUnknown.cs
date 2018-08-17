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
    class RequestSubstitutionWithUnknown : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var request = Get<InformationPartEvent>(p => !p.IsFilled);
            if (request == null || request.Subject == null)
                yield break;

            var unknownPhrases = GetUnknownPhrases(generator).ToArray();
            if (unknownPhrases.Length != 1)
                yield break;

            var phraseToAsk = unknownPhrases.First();

            CollectNewConceptLearning.GenerateActivationRequest(phraseToAsk, generator);
            yield return $"I don't know phrase {phraseToAsk}. What does it mean?";
        }
    }
}
