using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    abstract class EventBase
    {
        internal abstract void Accept(BeamGenerator g);

        public override string ToString()
        {
            var name = GetType().Name;

            return $"[{name}]";
        }
    }
}
