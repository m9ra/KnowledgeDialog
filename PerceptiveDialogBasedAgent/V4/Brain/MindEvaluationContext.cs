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

        internal readonly ConceptInstance EvaluatedConcept;

        internal MindEvaluationContext(ConceptInstance evaluatedConcept, MindState state)
        {
            _currentState = state;
            EvaluatedConcept = evaluatedConcept;
        }

        internal void Report(ConceptInstance instance)
        {
            throw new NotImplementedException();
        }

        internal PointableInstance GetParameter(Concept2 parameter)
        {
            return _currentState.GetPropertyValue(EvaluatedConcept, parameter);
        }

        internal void SetProperty(Concept2 property, PointableInstance value)
        {
            _currentState = _currentState.SetPropertyValue(EvaluatedConcept, property, value);
        }

        internal void AddScore(double score)
        {
            _currentState = _currentState.AddScore(score);
        }

        internal MindState EvaluateOnParametersComplete()
        {
            EvaluatedConcept.Concept.OnParametersComplete(this);
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

    }
}
