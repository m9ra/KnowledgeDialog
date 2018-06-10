using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    abstract class AbilityBase
    {
        /// <summary>
        /// Events that will be pushed into beam during initialization
        /// </summary>
        private readonly List<EventBase> _initializationEvents = new List<EventBase>();

        private bool _isInitialized = false;

        private Concept2 _currentConcept;

        private ConceptInstance _currentInstance;

        internal virtual void Register(AbilityBeamGenerator generator)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Can't initialize twice");

            _isInitialized = true;

            foreach (var evt in _initializationEvents)
            {
                generator.PushToAll(evt);
            }
        }

        internal void AddInitializationEvent(EventBase evt)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Can't change initialization in initialized state");

            _initializationEvents.Add(evt);
        }

        internal AbilityBase DefineConcept(Concept2 concept)
        {
            return DefineConcept(concept.Name);
        }

        internal AbilityBase DefineConcept(string conceptName, out Concept2 concept, bool fireConceptDefinedEvt = true)
        {
            _currentInstance = null;
            _currentConcept = concept = Concept2.From(conceptName);

            if (fireConceptDefinedEvt)
                AddInitializationEvent(new ConceptDefinedEvent(concept));

            return this;
        }

        internal AbilityBase DefineInstance(string conceptName, out Concept2 concept)
        {
            DefineConcept(conceptName, out concept);
            _currentConcept = null;
            _currentInstance = new ConceptInstance(concept);

            return this;
        }

        internal AbilityBase DefineConcept(string conceptName)
        {
            return DefineConcept(conceptName, out _);
        }

        internal AbilityBase Description(string description)
        {
            AddInitializationEvent(new ConceptDescriptionEvent(_currentConcept, description));
            return this;
        }

        internal AbilityBase Property(Concept2 instanceProperty, Concept2 value)
        {
            var target = _currentConcept == null ? new PropertySetTarget(_currentInstance, instanceProperty) : new PropertySetTarget(_currentConcept, instanceProperty);
            var valueInstance = new ConceptInstance(value);

            AddInitializationEvent(new PropertySetEvent(target, valueInstance));
            return this;
        }
    }
}
