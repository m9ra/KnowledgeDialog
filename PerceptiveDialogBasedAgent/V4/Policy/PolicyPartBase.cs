using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    abstract class PolicyPartBase
    {
        protected abstract IEnumerable<string> execute(BeamGenerator generator);

        private EventBase[] _previousTurnEvents;

        private EventBase[] _turnEvents;

        internal string[] Execute(BeamGenerator generator, EventBase[] previousTurnEvents, EventBase[] turnEvents)
        {
            try
            {
                _previousTurnEvents = previousTurnEvents;
                _turnEvents = turnEvents;

                return execute(generator).ToArray();
            }
            finally
            {
                _previousTurnEvents = null;
                _turnEvents = null;
            }
        }

        protected Evt Get<Evt>()
            where Evt : EventBase
        {
            for (var i = 0; i < _turnEvents.Length; ++i)
            {
                var e = _turnEvents[_turnEvents.Length - i - 1];
                if (e is Evt evt)
                    return evt;
            }
            return null;
        }

        protected string singular(ConceptInstance instance)
        {
            return instance.ToPrintable();
        }


        protected string plural(ConceptInstance instance)
        {
            return singular(instance) + "s";
        }
    }
}
