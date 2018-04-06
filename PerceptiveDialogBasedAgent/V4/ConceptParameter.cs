using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class ConceptParameter : PointableBase
    {
        internal readonly ConceptInstance Owner;

        internal readonly string Request;

        internal readonly bool AllowMultipleSubtitutions = false;

        private readonly ParameterRequirement[] _requirements;

        internal ConceptParameter(ConceptInstance owner, string request, params ParameterRequirement[] requirements) :
            this(owner, request, (IEnumerable<ParameterRequirement>)requirements)
        { }

        internal ConceptParameter(ConceptInstance owner, string request, IEnumerable<ParameterRequirement> requirements)
        {
            Owner = owner;
            Request = request;
            _requirements = requirements.ToArray();
        }

        internal bool IsAllowedForSubstitution(ConceptInstance instance, BodyState2 state)
        {
            if (!AllowMultipleSubtitutions && state.ContainsSubstitutionFor(this))
                // some substitution is present already
                return false;

            foreach (var requirement in _requirements)
            {
                if (!requirement.IsSatisfiedBy(instance, state))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var accumulator = Owner.GetHashCode();
            accumulator += AllowMultipleSubtitutions.GetHashCode();
            accumulator += _requirements.Select(r => r.GetHashCode()).Sum();
            return accumulator;
        }

        public override bool Equals(object obj)
        {
            var o = obj as ConceptParameter;
            if (o == null)
                return false;

            return
                Owner == o.Owner &&
                AllowMultipleSubtitutions == o.AllowMultipleSubtitutions &&
                Enumerable.SequenceEqual(_requirements, o._requirements);
        }

        public override string ToString()
        {
            return "Param: " + Request;
        }
    }
}
