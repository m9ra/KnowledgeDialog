using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class SubstitutionEvent : EventBase
    {
        internal readonly SubstitutionRequestEvent SubstitutionRequest;

        internal readonly CompleteInstanceEvent CompleteInstance;

        internal ConceptInstance Instance => CompleteInstance.Instance;

        public SubstitutionEvent(SubstitutionRequestEvent request, CompleteInstanceEvent evt)
        {
            SubstitutionRequest = request;
            CompleteInstance = evt;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[{SubstitutionRequest.TargetInstance.Concept.Name} <-- {CompleteInstance.Instance.Concept.Name}]";
        }
    }
}
