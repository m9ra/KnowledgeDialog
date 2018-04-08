using PerceptiveDialogBasedAgent.V3.Models;
using PerceptiveDialogBasedAgent.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    delegate void BodyAction(BodyContext context);

    class Body
    {
        private readonly ContextBeam _beam;

        private readonly ScoreModelBase _model;

        private readonly List<Concept> _concepts = new List<Concept>();

        private Concept _currentConcept = null;

        public IEnumerable<Concept> Concepts => _concepts;

        internal Body(ScoreModelBase model)
        {
            _beam = new ContextBeam(this);
            _model = model;
        }

        internal Body Concept(string conceptName, BodyAction action)
        {
            _currentConcept = new Concept(conceptName, action);
            _concepts.Add(_currentConcept);
            return this;
        }

        internal Concept GetConcept(string conceptName)
        {
            return _concepts.Where(c => c.Name == conceptName).FirstOrDefault();
        }

        internal BodyState GetBestState()
        {
            return _beam.GetBestState();
        }

        internal Body Description(string description)
        {
            _currentConcept.AddDescription(description);
            return this;
        }

        internal void AddInput(string word)
        {
            _beam.ExtendInput(word);
        }

        internal double PointingScore(BodyState currentState, InputPhrase phrase, Concept concept)
        {
            return _model.PointingScore(currentState, phrase, concept);
        }

        internal void SetState(BodyState state)
        {
            _beam.SetState(state);
        }

        internal double ReadoutScore(BodyState state)
        {
            return _model.ReadoutScore(state);
        }

        internal double ParameterAssignScore(BodyState currentState, ConceptRequirement parameter, Concept concept)
        {
            return _model.ParameterAssignScore(currentState, parameter, concept);
        }
    }
}
