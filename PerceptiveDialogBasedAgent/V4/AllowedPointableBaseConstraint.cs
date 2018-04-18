using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class AllowedPointableBaseConstraint : ParameterRequirement
    {
        private readonly HashSet<PointableInstance> _allowedConcepts;

        internal AllowedPointableBaseConstraint(IEnumerable<PointableInstance> allowedConcepts)
        {
            if (allowedConcepts != null)
                _allowedConcepts = new HashSet<PointableInstance>(allowedConcepts);
        }

        internal override bool IsSatisfiedBy(ConceptInstance instance, BodyState2 state)
        {
            return _allowedConcepts == null || _allowedConcepts.Contains(instance);
        }

        public override int GetHashCode()
        {
            return _allowedConcepts == null || !_allowedConcepts.Any() ? 0 : _allowedConcepts.Select(c => c.GetHashCode()).Aggregate((x, y) => x + 1);
        }

        public override bool Equals(object obj)
        {
            var o = obj as AllowedPointableBaseConstraint;
            if (o == null)
                return false;

            return _allowedConcepts == o._allowedConcepts || Enumerable.SequenceEqual(_allowedConcepts, o._allowedConcepts);
        }
    }
}
