using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.Abilities;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class LearnUnknownForRefinement : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Get<InformationReportEvent>();
            if (evt?.Instance.Concept != Concept2.NeedsRefinement)
                yield break;

            var instanceToRefine = generator.GetValue(evt.Instance, Concept2.Subject);
            var activationTarget = generator.GetValue(evt.Instance, Concept2.Target);

            var prefixesForRefine = generator.GetPrefixingUnknowns(instanceToRefine);
            if (!prefixesForRefine.TryGetValue(instanceToRefine, out var propertyUnknownRaw))
                //nothing to ask for
                yield break;

            var propertyUnknown = toMeaningfulPhrase(propertyUnknownRaw);
            if (propertyUnknown == "")
                yield break;

            /* 
              //TODO feature based questions
            
              //ACTION SEQUENCE
              //Do you want X which is UNKNOWN ?    * Prompt
              //-yes: what property UNKNOWN is ?    *   yes -> expect (properties)
              //-- expect property                  *
              //-no: So which X you want ?          *   no -> ?
              //--standard refinement               *

              yield return $"I know many {plural(instanceToRefine)} which one would you like?";*/


            var assignUnknownProperty = AssignUnknownProperty.Create(instanceToRefine, propertyUnknown, generator);

            YesNoPrompt.Generate(generator, assignUnknownProperty, instanceToRefine);
            yield return $"So, you would like to find {plural(instanceToRefine)} which are {propertyUnknown}?";
        }

        private string toMeaningfulPhrase(string phrase)
        {
            var auxiliaryWords = new[] { "a", "an", "the", "some", "my" };
            var input = phrase.ToLowerInvariant();
            var inputWords = Phrase.AsWords(input).ToList();

            while (inputWords.Count > 0 && auxiliaryWords.Contains(inputWords.First()))
                inputWords.RemoveAt(0);

            while (inputWords.Count > 0 && auxiliaryWords.Contains(inputWords.Last()))
                inputWords.RemoveAt(inputWords.Count - 1);

            return string.Join(" ", inputWords);
        }
    }
}
