using PerceptiveDialogBasedAgent.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    delegate void BodyAction2(BodyContext2 context);

    class BodyContext2
    {
        private BodyState2 _currentState;

        private readonly Body _body;

        internal BodyState2 CurrentState => _currentState;

        internal readonly ConceptInstance EvaluatedConcept;

        internal IEnumerable<Concept2> Databases => throw new NotImplementedException();

        public BodyContext2(ConceptInstance evaluatedConcept, Body body, BodyState2 inputState)
        {
            _body = body;
            _currentState = inputState;
            EvaluatedConcept = evaluatedConcept;
        }

        internal bool RequireParameter(string request, out PointableInstance parameter, IEnumerable<Concept2> domain = null)
        {
            var parameterConstraint = new AllowedConceptConstraint(domain);
            var parameterDefinition = new ConceptParameter(EvaluatedConcept, request, parameterConstraint);
            var substitution = _currentState.GetSubsitution(parameterDefinition);
            parameter = substitution?.FirstOrDefault() ?? null;
            if (!_currentState.IsDefined(parameterDefinition))
            {
                _currentState = _currentState.DefineParameter(parameterDefinition);
                return false;
            }

            return substitution != null;
        }

        internal bool RequireMultiParameter(string request, out IEnumerable<Concept2> parameter, IEnumerable<Concept2> domain = null)
        {
            throw new NotImplementedException();
        }

        internal void SetValue(PointableInstance target, PointableInstance index, PointableInstance value)
        {
            _currentState = _currentState.SetIndexValue(target, index, value);
        }

        internal IEnumerable<Concept2> GetCriterions(DatabaseHandler database)
        {
            var result = new List<Concept2>();

            foreach (var column in database.Columns)
            {
                foreach (var value in database.GetColumnValues(column))
                {
                    result.Add(_body.GetConcept(value));
                }
            }

            return result;
        }

        internal IEnumerable<Concept2> GetProperties()
        {
            //TODO property detection should be more complex - flags, properties on instances..
            var properties = new HashSet<Concept2>();
            foreach (var concept in _body.Concepts)
            {
                properties.UnionWith(concept.Properties);
            }

            return properties;
        }

        internal void AddScore(double score)
        {
            _currentState = _currentState.AddScore(score);
        }

        internal void Activate(PointableInstance activatedInstance)
        {
            //TODO activation has to be more natural
            _currentState = _currentState.Add(new[] { new RankedPointing(EvaluatedConcept, activatedInstance, 0.1) });
        }
    }
}
