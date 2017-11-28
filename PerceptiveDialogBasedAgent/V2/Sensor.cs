using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class Sensor
    {
        internal readonly string Name;

        private bool _isEnabled = false;

        internal Sensor(string name)
        {
            Name = name;
        }

        internal void Enable()
        {
            _isEnabled = true;
        }

        internal void Disable()
        {
            _isEnabled = false;
        }

        internal Constraints FillContext(Constraints constraints)
        {
            var conditionValue = _isEnabled ? Name : "~ " + Name;
            return constraints.AddCondition(conditionValue);
        }
    }
}
