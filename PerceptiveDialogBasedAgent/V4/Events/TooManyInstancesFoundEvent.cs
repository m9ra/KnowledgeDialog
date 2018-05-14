using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class TooManyInstancesFoundEvent : ResultEvent
    {
        internal readonly ConceptInstance Criterion;

        internal readonly SubstitutionRequestEvent SubstitutionRequest;

        public TooManyInstancesFoundEvent(ConceptInstance criterion, SubstitutionRequestEvent request)
        {
            Criterion = criterion;
            SubstitutionRequest = request;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }
    }
}
