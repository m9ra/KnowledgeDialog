using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class OutputEvent : EventBase
    {
        public readonly string OutputText;

        internal OutputEvent(string outputText)
        {
            OutputText = outputText;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"['{OutputText}']";
        }
    }
}
