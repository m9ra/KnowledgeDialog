using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    class RankedConcept
    {
        internal readonly Concept Concept;

        internal readonly double Rank;

        internal RankedConcept(Concept concept, double rank)
        {
            Concept = concept;
            Rank = rank;
        }
    }
}
