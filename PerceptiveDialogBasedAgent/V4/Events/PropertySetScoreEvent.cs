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
        internal readonly InformationPartEvent PropertySet;

        private readonly int _distancePenalty;

        internal PropertySetScoreEvent(InformationPartEvent propertySet, int distancePenalty)
        {
            PropertySet = propertySet;
            _distancePenalty = distancePenalty;
        }

        internal override IEnumerable<string> GenerateFeatures(BeamNode node)
        {
            var targetActivationEvent = BeamGenerator.GetInstanceActivationRequest(PropertySet.Subject, node);
            var sourceActivationEvent = BeamGenerator.GetInstanceActivationRequest(PropertySet.Value, node);

            if (targetActivationEvent == null || sourceActivationEvent == null)
                yield break;

            var ngramLimitCount = 2;
            var targetSufixes = new InputPhraseEvent[0];//BeamGenerator.GetSufixPhrases(targetActivationEvent.ActivationPhrase, ngramLimitCount, node);
            var targetPrefixes = BeamGenerator.GetPrefixPhrases(targetActivationEvent.ActivationPhrases.FirstOrDefault(), ngramLimitCount, node);
            var featureId = "* --" + PropertySet.Property.Name + "--> $1";
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
            return Configuration.ParameterSubstitutionScore / (1 + _distancePenalty);
        }
    }
}
