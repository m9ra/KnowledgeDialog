using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class DistanceScoreEvt : TracedScoreEventBase
    {
        private EventBase _evt1;
        private EventBase _evt2;

        public DistanceScoreEvt(EventBase evt1, EventBase evt2)
        {
            _evt1 = evt1;
            _evt2 = evt2;
        }

        internal override double GetDefaultScore()
        {
            return 0.1;
        }
    }
}
