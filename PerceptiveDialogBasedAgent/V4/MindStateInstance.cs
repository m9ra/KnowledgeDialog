using PerceptiveDialogBasedAgent.V4.Brain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class MindStateInstance : PointableInstance
    {
        internal readonly MindState State;

        internal MindStateInstance(MindState state) : base(null)
        {
            State = state;
        }

        internal override string ToPrintable()
        {
            throw new NotImplementedException();
        }
    }
}
