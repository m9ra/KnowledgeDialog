using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    class ConceptRequirement
    {
        internal readonly RankedConcept RequestingConcept;

        internal readonly string Request;

        internal readonly IEnumerable<Concept> Domain;

        internal ConceptRequirement(string request, RankedConcept requestingConcept, IEnumerable<Concept> domain)
        {
            Request = request;
            RequestingConcept = requestingConcept;
            Domain = domain.ToArray();
        }

        /// </inheritdoc>
        public override bool Equals(object obj)
        {
            var o = obj as ConceptRequirement;
            if (o == null)
                return false;

            return RequestingConcept.Equals(o.RequestingConcept) && Request.Equals(o.Request) && Enumerable.SequenceEqual(Domain, o.Domain);
        }

        /// </inheritdoc>
        public override int GetHashCode()
        {
            return RequestingConcept.GetHashCode() + Request.GetHashCode() + Domain.Sum(c => c.GetHashCode());
        }
    }
}
