using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    class MindState
    {
        private readonly Dictionary<MindField, object> _stateValues;

        internal readonly IEnumerable<ConceptInstance> Events;

        internal readonly ConceptInstance WorkingMemoryRoot;

        internal readonly PropertyContainer PropertyContainer;

        internal readonly double Score;

        private MindState(MindState previousState, double extraScore = 0, Dictionary<MindField, object> stateValues = null, PropertyContainer container = null, IEnumerable<ConceptInstance> events = null, ConceptInstance workingMemoryRoot = null)
        {
            var score = previousState?.Score ?? 0;

            Score = score + extraScore;
            PropertyContainer = container ?? previousState.PropertyContainer;
            _stateValues = stateValues ?? previousState._stateValues;
            WorkingMemoryRoot = workingMemoryRoot ?? previousState?.WorkingMemoryRoot;
            Events = events ?? previousState.Events;
        }

        internal MindState AddScore(double extraScore)
        {
            return new MindState(this, extraScore: extraScore);
        }

        internal PointableInstance GetPropertyValue(PointableInstance instance, Concept2 property)
        {
            return PropertyContainer.GetPropertyValue(instance, property);
        }

        internal MindState SetPropertyValue(PointableInstance instance, Concept2 property, Concept2 value)
        {
            var newContainer = PropertyContainer.SetPropertyValue(instance, property, value);
            return new MindState(this, container: newContainer);
        }

        internal MindState SetPropertyValue(PointableInstance instance, Concept2 property, PointableInstance value)
        {
            var newContainer = PropertyContainer.SetPropertyValue(instance, property, value);
            return new MindState(this, container: newContainer);
        }

        internal MindState AddEvent(ConceptInstance evt)
        {
            var newEvents = new List<ConceptInstance>(Events);
            newEvents.Add(evt);

            return new MindState(this, events: newEvents);
        }

        internal MindState Import(PointableInstance instance, PropertyContainer container)
        {
            var newContainer = PropertyContainer.Import(instance, container);
            return new MindState(this, container: newContainer);
        }

        internal IEnumerable<Concept2> GetProperties(PointableInstance instance)
        {
            return PropertyContainer.GetProperties(instance);
        }

        internal IEnumerable<Concept2> GetAvailableParameters(ConceptInstance instance)
        {
            return PropertyContainer.GetAvailableParameters(instance);
        }

        internal static MindState Empty(ConceptInstance workingMemoryRoot)
        {
            return new MindState(null, extraScore: 0, stateValues: new Dictionary<MindField, object>(), container: new PropertyContainer(), events: new ConceptInstance[0], workingMemoryRoot: workingMemoryRoot);
        }

        internal MindState SetValue<T>(MindField field, T value)
        {
            var newValues = new Dictionary<MindField, object>(_stateValues);
            newValues[field] = value;

            return new MindState(this, stateValues: newValues);
        }

        internal T GetValue<T>(MindField<T> field)
        {
            _stateValues.TryGetValue(field, out var value);
            return (T)value;
        }

        internal IEnumerable<SubstitutionPoint> GetSubstitutionPoints()
        {
            //traverse working memory tree and find missing parameters
            return collectSubstitutionPoints(WorkingMemoryRoot);
        }

        private IEnumerable<SubstitutionPoint> collectSubstitutionPoints(ConceptInstance instance)
        {
            var result = new List<SubstitutionPoint>();
            foreach (var property in GetProperties(instance))
            {
                if (!PropertyContainer.IsParameter(property))
                    continue;

                var value = PropertyContainer.GetPropertyValue(instance, property) as ConceptInstance;
                if (PropertyContainer.HasParameterSubstitution(instance, property))
                {
                    result.AddRange(collectSubstitutionPoints(value));
                }
                else
                {
                    result.Add(new SubstitutionPoint(instance, property, value, this, 0.1));//TODO consider
                }
            }

            return result;
        }
    }

    class MindField
    {
        internal readonly string Name;

        internal MindField(string name)
        {
            Name = name;
        }
    }

    class MindField<T> : MindField
    {
        internal MindField(string name)
            : base(name)
        {

        }
    }
}
