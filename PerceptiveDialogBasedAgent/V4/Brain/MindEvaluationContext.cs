using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    class MindEvaluationContext
    {
        private MindState _currentState;

        internal ConceptInstance EvaluatedConcept { get; private set; }

        internal MindState CurrentState => _currentState;

        internal MindEvaluationContext(ConceptInstance evaluatedConcept, MindState state)
        {
            _currentState = state;
            EvaluatedConcept = evaluatedConcept;
        }

        internal void Report(ConceptInstance instance)
        {
            throw new NotImplementedException();
        }

        internal PointableInstance GetProperty(Concept2 property)
        {
            return _currentState.GetPropertyValue(EvaluatedConcept, property);
        }

        internal PointableInstance GetProperty(PointableInstance target, Concept2 property)
        {
            return _currentState.GetPropertyValue(target, property);
        }

        internal void SetProperty(Concept2 property, PointableInstance value)
        {
            SetProperty(EvaluatedConcept, property, value);
        }

        internal void SetProperty(PointableInstance target, Concept2 property, PointableInstance value)
        {
            _currentState = _currentState.SetPropertyValue(target, property, value);
        }

        internal void AddScore(double score)
        {
            _currentState = _currentState.AddScore(score);
        }

        internal MindState EvaluateOnPropertyChange()
        {
            var currentConcept = EvaluatedConcept;
            while (currentConcept != null && currentConcept.Concept.OnPropertyChange == null)
            {
                currentConcept = GetParentConcept(currentConcept);
            }
            if (currentConcept == null)
                return _currentState;

            var initialConcept = EvaluatedConcept;
            EvaluatedConcept = currentConcept;
            EvaluatedConcept.Concept.OnPropertyChange?.Invoke(this);
            EvaluatedConcept = initialConcept;
            return _currentState;
        }

        internal ConceptInstance GetParentConcept(ConceptInstance instance)
        {
            return _currentState.PropertyContainer.GetParentConcept(instance);
        }

        internal MindState EvaluateOnParametersComplete()
        {
            EvaluatedConcept.Concept.OnParametersComplete?.Invoke(this);
            return _currentState;
        }

        internal MindState EvaluateOnExecution()
        {
            EvaluatedConcept.Concept.OnExecution(this);
            return _currentState;
        }

        internal void OutputEvent(PointableInstance output)
        {
            Event(Concept2.Output, Concept2.Subject, output);
        }

        internal void Event(Concept2 evt, Concept2 subject, PointableInstance constraint)
        {
            var evtInstance = new ConceptInstance(evt);
            _currentState = _currentState.SetPropertyValue(evtInstance, subject, constraint);

            Event(evtInstance);
        }

        internal void Event(ConceptInstance evt)
        {
            _currentState = _currentState.AddEvent(evt);
        }

        internal bool MeetsPattern(PointableInstance instance, ConceptInstance pattern)
        {
            return _currentState.PropertyContainer.MeetsPattern(instance, pattern);
        }

        internal IEnumerable<Concept2> GetPropertiesUsedFor(Concept2 targetConcept)
        {
            var result = new HashSet<Concept2>();
            foreach (var concept in _currentState.Body.Concepts)
            {
                foreach (var property in concept.Properties)
                {
                    var value = (concept.GetPropertyValue(property) as ConceptInstance)?.Concept;
                    if (value == targetConcept)
                    {
                        result.Add(property);
                        break;
                    }
                }
            }

            return result;
        }

        internal void Import(ConceptInstance instance, PropertyContainer container)
        {
            _currentState = _currentState.Import(instance, container);
        }

        internal void SideEffectInvocation(ConceptInstance instance)
        {
            if (instance.Concept.OnExecution == null)
                throw new InvalidOperationException("Cannot request invocation of given concept.");

            var invocation = new ConceptInstance(Concept2.Invocation);
            SetProperty(invocation, Concept2.Subject, instance);
            Event(invocation);
        }
    }
}

