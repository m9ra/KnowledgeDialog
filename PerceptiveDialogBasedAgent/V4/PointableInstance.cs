using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    abstract class PointableInstance
    {
        internal Phrase ActivationPhrase;

        internal abstract string ToPrintable();

        internal abstract IEnumerable<PointableInstance> GetPropertyValue(Concept2 property);

        internal PointableInstance(Phrase activationPhrase)
        {
            ActivationPhrase = activationPhrase;
        }
    }
}
