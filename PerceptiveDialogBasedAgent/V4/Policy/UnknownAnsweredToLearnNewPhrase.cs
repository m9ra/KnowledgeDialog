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
    class UnknownAnsweredToLearnNewPhrase : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var unknownPhrases = GetUnknownPhrases(generator);
            var isInLearnPhraseMode = PreviousPolicy<LearnNewPhrase>(out var learnPolicy) || PreviousPolicy<UnknownAnsweredToLearnNewPhrase>(out learnPolicy);

            if (!isInLearnPhraseMode || unknownPhrases.Count() != 1)
                yield break;

            var unknown = generator.GetValue(learnPolicy.Tag, Concept2.Unknown);
            var prompt = generator.GetValue(learnPolicy.Tag, Concept2.Prompt);


            //remember runtime info so others can use it
            generator.SetValue(TagInstance, Concept2.Unknown, unknown);
            generator.SetValue(TagInstance, Concept2.Prompt, prompt);

            generator.Push(new InstanceActivationRequestEvent(prompt));
            yield return $"Still, I'm not getting it. What does {singular(unknown)} mean?";
            yield return $"That is complicated. What does {singular(unknown)} mean?";
            yield return $"I don't know those words. Could you put {singular(unknown)} differently?";
        }
    }
}
