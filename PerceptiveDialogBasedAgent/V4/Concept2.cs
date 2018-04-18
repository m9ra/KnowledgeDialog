using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Concept2
    {
        public readonly string Name;

        public readonly bool IsNative;

        internal readonly BodyAction2 Action;

        internal IEnumerable<Concept2> Properties => _propertyValues.Keys;

        internal IEnumerable<string> Descriptions => _descriptions;

        private readonly List<string> _descriptions = new List<string>();

        private readonly Dictionary<Concept2, HashSet<PointableInstance>> _propertyValues = new Dictionary<Concept2, HashSet<PointableInstance>>();

        public Concept2(string name, BodyAction2 action, bool isNative)
        {
            Name = name;
            Action = action;
            IsNative = isNative;
        }

        public void AddDescription(string description)
        {
            _descriptions.Add(description);
        }

        public void AddPropertyValue(Concept2 property, PointableInstance value)
        {
            if (!_propertyValues.TryGetValue(property, out var valueSet))
                _propertyValues[property] = valueSet = new HashSet<PointableInstance>();

            valueSet.Add(value);
        }

        internal IEnumerable<PointableInstance> GetPropertyValue(Concept2 property)
        {
            _propertyValues.TryGetValue(property, out var values);
            return values;
        }


        /// </inheritdoc>
        public override string ToString()
        {
            return "'" + Name + "' D: " + _descriptions.Count;
        }
    }
}
