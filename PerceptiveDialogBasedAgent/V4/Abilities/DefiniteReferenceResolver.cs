using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class DefiniteReferenceResolver : ConceptAbilityBase
    {
        private readonly Concept2 _parameter = Concept2.Subject;

        internal DefiniteReferenceResolver()
            : base("the")
        {
            AddParameter(_parameter);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var parameterValue = generator.GetValue(instance, _parameter);
            var relevantInstances = getRelevantInstances(instance, parameterValue.Concept, generator).ToArray();

            foreach (var relevantInstance in relevantInstances)
            {
                //avoid reference circles
                if (relevantInstance.Concept == instance.Concept)
                    continue;

                //try tunnel instances between turns
                generator.Push(new StaticScoreEvent(0.05));
                generator.Push(new InstanceReferencedEvent(relevantInstance));
                generator.Pop();
                generator.Pop();
            }
        }

        private IEnumerable<ConceptInstance> getRelevantInstances(ConceptInstance instance, Concept2 relevanceCriterion, BeamGenerator beam)
        {
            var relevantCandidates = beam.GetInstances();
            foreach (var relevantCandidate in relevantCandidates)
            {
                if (relevantCandidate == instance)
                    // prevent self reference
                    continue;

                var values = beam.GetPropertyValues(relevantCandidate);
                var isRelevant = relevantCandidate.Concept == relevanceCriterion || values.Any(v => v.Key == relevanceCriterion || v.Value.Concept == relevanceCriterion);

                if (isRelevant)
                    yield return relevantCandidate;
            }
        }
    }
}
