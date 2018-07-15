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
    class LearnPropertyValue : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var definedConcepts = new HashSet<Concept2>(generator.GetDefinedConcepts());
            var conceptWithAssignedProperty = Get<PropertySetEvent>((s) => hasExtraInformation(s, definedConcepts, generator));
            if (conceptWithAssignedProperty == null)
                yield break;

            var target = conceptWithAssignedProperty.Target;
            var rememberValue = RememberPropertyValue.Create(generator, target, conceptWithAssignedProperty.SubstitutedValue);

            YesNoPrompt.Generate(generator, rememberValue, new ConceptInstance(Concept2.Nothing));
            yield return $"You think that {singular(target.Instance)} has {singular(conceptWithAssignedProperty.SubstitutedValue)} {singular(target.Property)}?";
        }

        private bool hasExtraInformation(PropertySetEvent setEvent, HashSet<Concept2> definedConcepts, BeamGenerator generator)
        {
            var instance = setEvent.Target.Instance;
            if (instance == null)
                return false;

            return
                definedConcepts.Contains(setEvent.Target.Property) &&
                definedConcepts.Contains(setEvent.Target.Instance.Concept) &&
                generator.IsKnownPropertyOf(setEvent.Target.Instance, setEvent.Target.Property)
                ;
        }
    }
}
