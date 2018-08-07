using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class EssentialKnowledge : KnowledgeAbilityBase
    {
        public EssentialKnowledge()
        {
            DefineConcept(Concept2.Yes)
                .Description("sure")
                .Description("of course")
                .Description("indeed");
            
            DefineConcept(Concept2.No);
            DefineConcept(Concept2.DontKnow);
            DefineConcept(Concept2.Nothing)
                .Description("none");

            DefineConcept(Concept2.Property);
        }
    }
}
