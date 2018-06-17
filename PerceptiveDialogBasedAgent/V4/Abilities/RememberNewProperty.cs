using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class RememberNewProperty : ConceptAbilityBase
    {
        internal RememberNewProperty() : base("remember")
        {
            AddParameter(Concept2.Target);
            AddParameter(Concept2.Property);
            AddParameter(Concept2.Subject);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var target = generator.GetValue(instance, Concept2.Target);
            var property = generator.GetValue(instance, Concept2.Property);
            var subject = generator.GetValue(instance, Concept2.Subject);
            //throw new NotImplementedException();
        }
    }
}
