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
            DefineConcept(Concept2.Yes);
            DefineConcept(Concept2.No);
            DefineConcept(Concept2.DontKnow);
        }
    }
}
