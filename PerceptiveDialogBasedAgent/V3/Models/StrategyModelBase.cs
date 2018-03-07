using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3.Models
{
    abstract class StrategyModelBase
    {
        internal abstract BodyState InputProcessingBypass(BodyState state, string input);

        internal abstract BodyState NoOutput(BodyState state);
        internal abstract BodyState AfterReadout(BodyState state);
    }
}
