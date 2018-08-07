using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    [Serializable]
    abstract class PointableInstance
    {
        internal readonly Phrase ActivationPhrase;

        internal abstract string ToPrintable();

        internal PointableInstance(Phrase activationPhrase)
        {
            ActivationPhrase = activationPhrase;
        }
    }
}
