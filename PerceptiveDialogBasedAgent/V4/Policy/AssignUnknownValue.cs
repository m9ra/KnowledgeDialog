﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class AssignUnknownValue : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var unknownPhrases = getUnknownPhrases(generator);
            var substitutionRequest = Get<InformationPartEvent>(p => !p.IsFilled, searchInsideTurnOnly: false);
            if (unknownPhrases.Count() != 1 || substitutionRequest == null)
                yield break;

            var unknownPhrase = unknownPhrases.FirstOrDefault();
            var assignUnknownProperty = new ConceptInstance(Concept2.AssignUnknownProperty);
            var unknownPropertyCandidate = new ConceptInstance(Concept2.From(unknownPhrase));
            generator.SetValue(assignUnknownProperty, Concept2.Subject, unknownPropertyCandidate);

            //TODO incorporate target property
            generator.SetValue(assignUnknownProperty, Concept2.Target, substitutionRequest.Subject);
            generator.Push(new InstanceActivationRequestEvent(assignUnknownProperty));

            yield return $"What does {unknownPhrase} mean?";
        }

        private IEnumerable<string> getUnknownPhrases(BeamGenerator generator)
        {
            var allInputPhrases = GetMany<InputPhraseEvent>();

            var currentBuffer = new List<InputPhraseEvent>();
            foreach (var inputPhrase in allInputPhrases)
            {
                var phrase = inputPhrase;
                if (generator.IsInputUsed(phrase))
                {
                    if (currentBuffer.Count > 0)
                        yield return composeUnknownPhrase(currentBuffer);

                    currentBuffer.Clear();
                }

                currentBuffer.Add(phrase);
            }

            if (currentBuffer.Count > 0)
                yield return composeUnknownPhrase(currentBuffer);
        }

        private string composeUnknownPhrase(IEnumerable<InputPhraseEvent> currentBuffer)
        {
            return string.Join(" ", currentBuffer.Select(i => i.Phrase));
        }
    }
}
