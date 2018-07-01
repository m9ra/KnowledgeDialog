using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    abstract class TracedScoreEventBase : EventBase
    {
        internal abstract double GetDefaultScore(BeamNode node);

        internal abstract IEnumerable<string> GenerateFeatures(BeamNode node);

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[+{GetDefaultScore(null):0.00}]";
        }
    }
}
