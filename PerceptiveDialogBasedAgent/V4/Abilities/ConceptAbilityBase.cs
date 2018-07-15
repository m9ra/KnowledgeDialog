using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    abstract class ConceptAbilityBase : AbilityBase
    {
        protected abstract void onInstanceActivated(ConceptInstance instance, BeamGenerator generator);

        internal Concept2 AbilityConcept;

        internal ConceptAbilityBase(string conceptName, bool fireConceptDefinedEvt = true)
        {
            DefineConcept(conceptName, out AbilityConcept, fireConceptDefinedEvt);
        }

        internal ConceptAbilityBase(Concept2 concept, bool fireConceptDefinedEvt = false)
            : this(concept.Name, fireConceptDefinedEvt)
        {

        }

        protected void AddParameter(Concept2 parameter)
        {
            var pattern = new ConceptInstance(Concept2.Something);
            AddInitializationEvent(new ParamDefinedEvent(AbilityConcept, parameter, pattern));
        }

        internal override void Register(AbilityBeamGenerator generator)
        {
            base.Register(generator);

            generator.AddCallback(AbilityConcept, abilityCallback);
        }

        private void abilityCallback(ConceptInstance instance, BeamGenerator generator)
        {
            onInstanceActivated(instance, generator);
        }
    }
}
