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

        internal void AddScore(double score)
        {
            _currentState = _currentState.AddScore(score);
        }

        internal MindState Evaluate()
        {
            EvaluatedConcept.Concept.Action(this);
            return _currentState;
        }
    }
}
