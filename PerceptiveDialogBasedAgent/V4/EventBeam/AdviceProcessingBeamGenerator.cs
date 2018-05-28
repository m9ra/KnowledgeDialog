using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class AdviceProcessingBeamGenerator : RestaurantDomainBeamGenerator
    {
        public AdviceProcessingBeamGenerator()
        {
            DefineConcept("pizza");
            DefineConcept("food");
            var instanceOf = DefineConcept(Concept2.InstanceOf);
            DefineParameter(instanceOf, Concept2.Target, new ConceptInstance(Concept2.Something));
            DefineParameter(instanceOf, Concept2.Subject, new ConceptInstance(Concept2.Something));

            AddDescription(instanceOf, "is");
            AddDescription(instanceOf, "superclasses");
            AddDescription(instanceOf, "is class of");

            AddFeatureScore("superclasses $1 | * --subject--> $1", 1.0);
            AddFeatureScore("yes $1 | * --subject--> $1", 1.0);
            AddFeatureScore("yes it $1 | * --target--> $1", 1.0);
            AddCallback(Concept2.InstanceOf, _instanceOf);
        }

        private void _instanceOf(ConceptInstance action, ExecutionBeamGenerator generator)
        {
            //throw new NotImplementedException();
        }
    }
}
