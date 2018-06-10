using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Concept2
    {
        private readonly Dictionary<Concept2, PointableInstance> _propertyValues = new Dictionary<Concept2, PointableInstance>();

        private static readonly Dictionary<string, Concept2> _definedConcepts = new Dictionary<string, Concept2>();

        public readonly static Concept2 OnSetListener = Concept("on set listener");
        public readonly static Concept2 DisambiguationFailed = Concept("disambiguation failed");
        public readonly static Concept2 PropertyValueDisambiguation = Concept("property value disambiguation");
        public readonly static Concept2 AssignUnknownProperty = Concept("assign unknown property");
        public readonly static Concept2 FireEvent = Concept("fire event");
        public readonly static Concept2 ActivationRequestEvent = Concept("activation request event");
        public readonly static Concept2 Answer = Concept("answer");
        public readonly static Concept2 Prompt = Concept("prompt");
        public readonly static Concept2 InstanceOf = Concept("instance of");
        public readonly static Concept2 NewTurn = Concept("new turn");
        public readonly static Concept2 NotFound = Concept("not found");
        public readonly static Concept2 NeedsRefinement = Concept("refinement");
        public readonly static Concept2 CompleteAction = Concept("complete action");
        public readonly static Concept2 NativeAction = Concept("native action").AddDescription("Describes action that agent can do");
        public readonly static Concept2 Something = Concept("something").AddDescription("placeholder for some concept");
        public readonly static Concept2 Parameter = Concept("parameter").AddDescription("represents parameter of an action");
        public readonly static Concept2 Yes = Concept("yes").AddDescription("positive answer to a question");
        public readonly static Concept2 YesExplicit = Concept("yes").AddDescription("yes it is");
        public readonly static Concept2 No = Concept("no").AddDescription("negative answer to a question");
        public readonly static Concept2 DontKnow = Concept("dont know").AddDescription("i dont know").AddDescription("dunno");
        public readonly static Concept2 It = Concept("it").AddDescription("reference");
        public readonly static Concept2 Output = Concept("output");
        public readonly static Concept2 Subject = Concept("subject").SetPropertyValue(Parameter, new ConceptInstance(Yes));
        public readonly static Concept2 Property = Concept("property");
        public readonly static Concept2 Unknown = Concept("unknown");
        public readonly static Concept2 Target = Concept("target");
        public readonly static Concept2 TargetProperty = Concept("target property");
        public readonly static Concept2 StateToRetry = Concept("state to retry");
        public readonly static Concept2 SubstitutionRequestedEvent = Concept("substitution requested");
        public readonly static Concept2 ConceptName = Concept("concept name");
        public readonly static Concept2 Invocation = Concept("invocation").SetPropertyValue(Subject, new ConceptInstance(Concept2.Something));
        public readonly static Concept2 ActionToExecute = Concept("action to execute");

        public readonly string Name;

        public readonly bool IsNative;

        internal IEnumerable<Concept2> Properties => _propertyValues.Keys;

        internal IEnumerable<string> Descriptions => _descriptions;

        private readonly List<string> _descriptions = new List<string>();

        private Concept2(string name, bool isNative = true)
        {
            Name = name;
            IsNative = isNative;
        }

        public static Concept2 From(string name, bool isNative = true)
        {
            if (!_definedConcepts.TryGetValue(name, out var concept))
            {
                _definedConcepts[name] = concept = new Concept2(name, isNative);
            }

            if (concept.IsNative != isNative)
                throw new InvalidOperationException("Cannot change concept nativness");

            return concept;
        }

        public Concept2 AddDescription(string description)
        {
            _descriptions.Add(description);

            return this;
        }

        public static Concept2 Concept(string name)
        {
            return From(name, true);
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
