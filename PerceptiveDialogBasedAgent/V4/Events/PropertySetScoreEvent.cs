using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V2;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class PropertySetScoreEvent : TracedScoreEventBase
    {
        internal readonly PropertySetEvent PropertySet;

        internal PropertySetScoreEvent(PropertySetEvent propertySet)
        {
            PropertySet = propertySet;
        }

        internal override IEnumerable<string> GenerateFeatures(BeamNode node)
        {
            var targetActivationEvent = BeamGenerator.GetInstanceActivation(PropertySet.Target.Instance, node);
            var sourceActivationEvent = BeamGenerator.GetInstanceActivation(PropertySet.SubstitutedValue, node);

            if (targetActivationEvent == null || sourceActivationEvent == null)
                yield break;

            var ngramLimitCount = 2;
            var targetSufixes = new InputPhraseEvent[0];//BeamGenerator.GetSufixPhrases(targetActivationEvent.ActivationPhrase, ngramLimitCount, node);
            var targetPrefixes = BeamGenerator.GetPrefixPhrases(targetActivationEvent.ActivationPhrase, ngramLimitCount, node);
            var featureId = "* --" + PropertySet.Target.Property.Name + "--> $1";
            var targetId = "$1";

            for (var i = 0; i < ngramLimitCount; ++i)
            {
                if (targetSufixes.Length > i)
                    yield return targetId + " " + ngramFeature(targetSufixes, i) + " | " + featureId;

                if (targetPrefixes.Length > i)
                    yield return ngramFeature(targetPrefixes, i) + " " + targetId + " | " + featureId;
            }
        }

        private string ngramFeature(InputPhraseEvent[] phrases, int n)
        {
            var result = string.Join(" ", phrases.Take(n + 1).Select(p => p.Phrase));
            return result;
        }

        internal override double GetDefaultScore(BeamNode node)
        {
            return Configuration.ParameterSubstitutionScore;
        }
    }
}
