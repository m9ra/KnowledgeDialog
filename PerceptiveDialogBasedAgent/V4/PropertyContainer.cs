using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class PropertyContainer
    {
        private readonly Dictionary<Tuple<PointableInstance, Concept2>, PointableInstance> _propertyValues = new Dictionary<Tuple<PointableInstance, Concept2>, PointableInstance>();

        private PropertyContainer(Dictionary<Tuple<PointableInstance, Concept2>, PointableInstance> propertyValues)
        {
            _propertyValues = propertyValues;
        }

        internal PropertyContainer()
        {

        }

        internal bool ContainsKey(Tuple<PointableInstance, Concept2> key)
        {
            return _propertyValues.ContainsKey(key);
        }

        internal bool ContainsSubstitutionFor(PointableInstance container, Concept2 parameter)
        {
            var key = Tuple.Create(container, parameter);
            return _propertyValues.TryGetValue(key, out var substitutions) && substitutions != null;
        }

        internal PropertyContainer SetPropertyValue(PointableInstance target, Concept2 property, Concept2 value)
        {
            return SetPropertyValue(target, property, new ConceptInstance(value));
        }

        internal PropertyContainer SetPropertyValue(PointableInstance target, Concept2 property, PointableInstance value)
        {
            var key = Tuple.Create(target, property);
            var newPropertyValues = new Dictionary<Tuple<PointableInstance, Concept2>, PointableInstance>(_propertyValues);
            newPropertyValues[key] = value;

            return new PropertyContainer(newPropertyValues);
        }

        internal PointableInstance GetPropertyValue(PointableInstance instance, Concept2 property)
        {
            if (!_propertyValues.TryGetValue(Tuple.Create(instance, property), out var result))
            {
                var conceptInstance = instance as ConceptInstance;
                result = conceptInstance?.Concept.GetPropertyValue(property);
            }
            return result;
        }

        internal PropertyContainer Import(PointableInstance instance, PropertyContainer container)
        {
            var newValues = new Dictionary<Tuple<PointableInstance, Concept2>, PointableInstance>(_propertyValues);
            foreach (var key in container._propertyValues.Keys)
            {
                if (key.Item1 == instance)
                    newValues[key] = container._propertyValues[key];
            }

            return new PropertyContainer(newValues);
        }

        internal IEnumerable<Concept2> GetProperties(PointableInstance instance)
        {
            var result = new HashSet<Concept2>();
            foreach (var key in _propertyValues.Keys)
            {
                if (key.Item1 == instance)
                    result.Add(key.Item2);
            }

            var conceptInstance = instance as ConceptInstance;
            if (conceptInstance != null)
                result.UnionWith(conceptInstance.Concept.Properties);

            return result;
        }

        internal IEnumerable<Concept2> GetAvailableParameters(ConceptInstance instance)
        {
            var result = new List<Concept2>();
            foreach (var property in GetProperties(instance))
            {
                if (!IsParameter(property))
                    continue;

                if (HasParameterSubstitution(instance, property))
                    continue;

                result.Add(property);
            }

            return result;
        }

        internal bool IsParameter(Concept2 property)
        {
            var value = property.GetPropertyValue(Concept2.Parameter) as ConceptInstance;
            return value?.Concept == Concept2.Yes;
        }

        internal bool HasParameterSubstitution(PointableInstance instance, Concept2 parameter)
        {
            var conceptInstance = instance as ConceptInstance;
            if (conceptInstance is null)
                throw new NotImplementedException();

            var currentValue = GetPropertyValue(instance, parameter);
            return currentValue != conceptInstance.Concept.GetPropertyValue(parameter);
        }

        internal bool MeetsPattern(PointableInstance instance, ConceptInstance pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");

            var o = instance as ConceptInstance;
            if (o == null)
                return false;

            //TODO something semantic and other conditionals should be refactored somewhere
            var isRootLevelMatch = o.Concept == pattern.Concept || pattern.Concept == Concept2.Something;
            if (!isRootLevelMatch)
                return false;

            foreach (var property in GetProperties(pattern))
            {
                if (!MeetsPattern(GetPropertyValue(instance, property), GetPropertyValue(pattern, property) as ConceptInstance))
                    return false;
            }

            return true;
        }
    }
}
