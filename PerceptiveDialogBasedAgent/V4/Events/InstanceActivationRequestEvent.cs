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
        internal readonly InputPhraseEvent[] ActivationPhrases;

        public InstanceActivationRequestEvent(InputPhraseEvent[] activationPhrases, ConceptInstance conceptInstance)
        {
            ActivationPhrases = activationPhrases ?? throw new ArgumentNullException(nameof(activationPhrases));
            Instance = conceptInstance;
        }

        public InstanceActivationRequestEvent(ConceptInstance conceptInstance)
        {
            ActivationPhrases = new InputPhraseEvent[0];
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
