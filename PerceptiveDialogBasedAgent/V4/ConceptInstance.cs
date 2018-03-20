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
        internal readonly Concept2 Concept;

        internal ConceptInstance(Concept2 concept)
        {
            Concept = concept;
        }
    }
}
