using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class AllowedConceptConstraint : ParameterRequirement
    {
        private readonly HashSet<Concept2> _allowedConcepts;

        internal AllowedConceptConstraint(IEnumerable<Concept2> allowedConcepts)
        {
            if (allowedConcepts != null)
                _allowedConcepts = new HashSet<Concept2>(allowedConcepts);
        }

        internal override bool IsSatisfiedBy(ConceptInstance instance, BodyState2 state)
        {
            return _allowedConcepts == null || _allowedConcepts.Contains(instance.Concept);
        }

        public override int GetHashCode()
        {
            return _allowedConcepts == null ? 0 : _allowedConcepts.Select(c => c.GetHashCode()).Sum();
        }

        public override bool Equals(object obj)
        {
            var o = obj as AllowedConceptConstraint;
            if (o == null)
                return false;

            return _allowedConcepts == o._allowedConcepts || Enumerable.SequenceEqual(_allowedConcepts, o._allowedConcepts);
        }
    }
}
