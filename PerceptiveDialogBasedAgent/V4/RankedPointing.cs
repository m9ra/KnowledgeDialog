using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class RankedPointing
    {
        internal readonly PointableBase Target;

        internal readonly double Rank;

        internal readonly InputPhrase InputPhrase;

        internal RankedPointing(InputPhrase inputPhrase, PointableBase target, double rank)
        {
            Target = target;
            Rank = rank;
            InputPhrase = inputPhrase;
        }

        public override string ToString()
        {
            return $"{Target} {Rank:0.00}";
        }
    }
}
