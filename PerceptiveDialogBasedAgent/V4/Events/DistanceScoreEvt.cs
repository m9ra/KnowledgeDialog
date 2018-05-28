using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class DistanceScoreEvt : TracedScoreEventBase
    {
        private readonly EventBase _evt1;

        private readonly EventBase _evt2;

        public DistanceScoreEvt(EventBase evt1, EventBase evt2)
        {
            _evt1 = evt1;
            _evt2 = evt2;
        }

        internal override double GetDefaultScore(BeamNode node)
        {
            return 0.1;
        }

        internal override IEnumerable<string> GenerateFeatures(BeamNode node)
        {
            yield break;
        }
    }
}
