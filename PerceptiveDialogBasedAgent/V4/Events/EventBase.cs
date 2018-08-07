using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    [Serializable]
    abstract class EventBase
    {
        internal abstract void Accept(BeamGenerator g);

        public override string ToString()
        {
            var name = GetType().Name;

            return $"[{name}]";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            //TODO revise this
            var o = obj as EventBase;
            if (o == null)
                return false;

            return ToString() == o.ToString();
        }
    }
}
