using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class StaticScoreEvent : TracedScoreEventBase
    {
        internal readonly double Score;

        internal StaticScoreEvent(double score)
        {
            Score = score;
        }

        internal override double GetDefaultScore(BeamNode node)
        {
            return Score;
        }

        internal override IEnumerable<string> GenerateFeatures(BeamNode node)
        {
            yield break;
        }
    }
}
