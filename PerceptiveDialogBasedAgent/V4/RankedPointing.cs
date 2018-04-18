﻿using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class RankedPointing
    {
        internal readonly PointableInstance Target;

        internal readonly double Rank;

        internal readonly PointableInstance Source;

        internal RankedPointing(PointableInstance source, PointableInstance target, double rank)
        {
            Target = target;
            Rank = rank;
            Source = source;
        }

        public override string ToString()
        {
            return $"{Source}-->{Target} {Rank:0.00}";
        }
    }
}
