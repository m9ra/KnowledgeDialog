using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    class InstanceContext
    {
        private BodyState2 _currentState;

        internal InstanceContext(BodyState2 initialState)
        {
            _currentState = initialState;
        }
    }
}
