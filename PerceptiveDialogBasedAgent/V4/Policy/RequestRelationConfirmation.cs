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
    class RequestRelationConfirmation : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var fullRelation = Get<PropertySetEvent>(p => IsDefined(p.SubstitutedValue.Concept) && IsDefined(p.Target.Instance?.Concept));
            if (fullRelation == null)
                yield break;

            var targetInstance = fullRelation.Target.Instance;
            if (targetInstance == null)
                yield break;

            var children = generator.GetInverseConceptValues(Concept2.InstanceOf, targetInstance);
            if (children.Any())
                //TODO add learning for classes
                yield break;

            var parameters = generator.GetParameterDefinitions(targetInstance);
            if (parameters.Any(p => p.Property == fullRelation.Target.Property))
                //don't learn argument values
                yield break;

            generator.Push(new StaticScoreEvent(0.1));
            var remember = RememberPropertyValue.Create(generator, fullRelation.Target, fullRelation.SubstitutedValue);
            YesNoPrompt.Generate(generator, remember, new ConceptInstance(Concept2.Nothing));
            yield return $"So you think, {singularWithProperty(fullRelation.Target.Instance)} ?";
        }
    }
}
