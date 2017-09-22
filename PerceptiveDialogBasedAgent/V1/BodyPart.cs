using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V1
{
    class BodyPart
    {

    }

    class Sensor : BodyPart
    {
        /// <summary>
        /// Trigger that will cause run of the actions.
        /// </summary>
        public readonly string Trigger;

        /// <summary>
        /// Actions that will be triggered.
        /// </summary>
        public readonly List<string> Actions = new List<string>();

        internal Sensor(string trigger)
        {
            Trigger = trigger;
        }
    }

    class Actor : BodyPart
    {
        /// <summary>
        /// Name of the actor.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Phrase which is used for connection between mind and native action.
        /// </summary>
        public readonly string PhrasePattern;

        /// <summary>
        /// The native call executiong an action.
        /// </summary>
        public readonly NativeCallWrapper Call;
    }

}
