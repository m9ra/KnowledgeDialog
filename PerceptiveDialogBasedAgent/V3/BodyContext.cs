using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    class BodyContext
    {
        private BodyState _currentState;

        private readonly Body _body;

        internal BodyState CurrentState => _currentState;

        internal readonly RankedConcept EvaluatedConcept;

        internal IEnumerable<Concept> Databases => throw new NotImplementedException();

        public BodyContext(RankedConcept evaluatedConcept, Body body, BodyState inputState)
        {
            _body = body;
            _currentState = inputState;
            EvaluatedConcept = evaluatedConcept;
        }

        internal bool RequireParameter(string request, out Concept parameter, IEnumerable<Concept> domain = null)
        {
            var realDomain = getRealDomain(domain);
            var requirement = new ConceptRequirement(request, EvaluatedConcept, realDomain);
            parameter = _currentState.GetParameter(requirement);
            if (parameter == null)
            {
                _currentState = _currentState.AddRequirement(requirement);
            }

            return parameter != null;
        }

        internal bool RequireMultiParameter(string request, out IEnumerable<Concept> parameter, IEnumerable<Concept> domain = null)
        {
            var realDomain = getRealDomain(domain);
            var requirement = new ConceptRequirement(request, EvaluatedConcept, realDomain);
            parameter = _currentState.GetMultiParameter(requirement);
            if (parameter == null)
            {
                _currentState = _currentState.AddMultiRequirement(requirement);
            }

            return parameter != null;
        }

        internal void SetValue(string variable, string value)
        {
            _currentState = _currentState.SetValue(variable, value);
        }

        internal IEnumerable<Concept> GetCriterions(Concept database)
        {
            throw new NotImplementedException();
        }

        internal void EvaluateActivation(Concept concept)
        {
            concept?.Action(this);
        }

        private IEnumerable<Concept> getRealDomain(IEnumerable<Concept> domain)
        {
            if (domain == null)
                return _body.Concepts;

            return domain;
        }

    }
}
