using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    abstract class PointingModelBase
    {
        internal abstract IEnumerable<RankedPointing> GenerateMappings(BodyState2 state);

        internal abstract IEnumerable<RankedPointing> GetForwardings(ConceptInstance forwardedConcept, BodyState2 state);

        internal abstract BodyState2 AddSubstitution(BodyState2 state, ConceptParameter parameter, ConceptInstance value);

        internal abstract BodyState2 StateReaction(BodyState2 state);

        internal abstract void OnConceptChange();
    
    }
}
