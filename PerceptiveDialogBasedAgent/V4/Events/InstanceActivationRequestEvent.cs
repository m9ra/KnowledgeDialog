using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class InstanceActivationRequestEvent : EventBase
    {
        internal readonly ConceptInstance Instance;
        internal readonly InputPhraseEvent ActivationPhrase;

        public InstanceActivationRequestEvent(InputPhraseEvent activationPhrase, ConceptInstance conceptInstance)
        {
            ActivationPhrase = activationPhrase;
            Instance = conceptInstance;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[req: {Instance.Concept.Name}]";
        }
    }
}
