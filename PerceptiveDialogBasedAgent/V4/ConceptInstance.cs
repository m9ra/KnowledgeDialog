using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class ConceptInstance : PointableBase
    {
        internal readonly Concept Concept;

        internal ConceptInstance(Concept concept)
        {
            Concept = concept;
        }
    }
}
