using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Concept2
    {
        public readonly static Concept2 InstanceOf = Concept("instance of");
        public readonly static Concept2 NewTurn = Concept("new turn");
        public readonly static Concept2 NotFoundEvent = Concept("not found");
        public readonly static Concept2 NeedsRefinement = Concept("refinement");
        public readonly static Concept2 CompleteAction = Concept("complete action");
        public readonly static Concept2 NativeAction = Concept("native action").AddDescription("Describes action that agent can do");
        public readonly static Concept2 Something = Concept("something").AddDescription("placeholder for some concept");
        public readonly static Concept2 Parameter = Concept("parameter").AddDescription("represents parameter of an action");
        public readonly static Concept2 Yes = Concept("yes").AddDescription("positive answer to a question");
        public readonly static Concept2 No = Concept("no").AddDescription("negative answer to a question");
        public readonly static Concept2 Output = Concept("output");
        public readonly static Concept2 Subject = Concept("subject").SetPropertyValue(Parameter, new ConceptInstance(Yes));
        public readonly static Concept2 Target = Concept("target");




        public readonly string Name;

        public readonly bool IsNative;

        internal readonly MindAction OnPropertyChange;

        internal readonly MindAction OnParametersComplete;

        internal readonly MindAction OnExecution;

        internal IEnumerable<Concept2> Properties => _propertyValues.Keys;

        internal IEnumerable<string> Descriptions => _descriptions;

        private readonly List<string> _descriptions = new List<string>();

        private readonly Dictionary<Concept2, PointableInstance> _propertyValues = new Dictionary<Concept2, PointableInstance>();


        public Concept2(string name, MindAction onParametersComplete, MindAction onExecution, MindAction onPropertyChange, bool isNative)
        {
            Name = name;
            OnParametersComplete = onParametersComplete;
            OnPropertyChange = onPropertyChange;
            OnExecution = onExecution;
            IsNative = isNative;
        }

        public Concept2 AddDescription(string description)
        {
            _descriptions.Add(description);

            return this;
        }

        public static Concept2 Concept(string name)
        {
            return new Concept2(name, null, null, null, true);
        }

        public Concept2 SetPropertyValue(Concept2 property, PointableInstance value)
        {
            _propertyValues[property] = value;
            return this;
        }

        internal PointableInstance GetPropertyValue(Concept2 property)
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
