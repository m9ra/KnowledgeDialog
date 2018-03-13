using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    abstract class PointingGeneratorBase
    {
        internal abstract IEnumerable<RankedPointing> GenerateMappings(BodyState2 state);
    }
}
