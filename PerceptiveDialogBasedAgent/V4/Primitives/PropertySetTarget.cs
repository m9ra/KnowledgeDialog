using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Primitives
{
    class PropertySetTarget
    {
        /// <summary>
        /// Property set for a particular instance.
        /// </summary>
        public readonly ConceptInstance Instance;

        /// <summary>
        /// Property set for all instances of the concept.
        /// </summary>
        public readonly Concept2 Concept;

        public readonly Concept2 Property;

        public PropertySetTarget(ConceptInstance instance, Concept2 property)
        {
            Instance = instance;
            Property = property;
        }

        public PropertySetTarget(Concept2 concept, Concept2 property)
        {
            Concept = concept;
            Property = property;
        }

        public string TargetRepresentation()
        {
            if (Instance == null)
                return Concept.Name;

            return Instance.ToPrintable();
        }

        public override int GetHashCode()
        {
            var acc = 0;
            if (Concept != null)
                acc += Concept.GetHashCode();

            if (Property != null)
                acc += Property.GetHashCode();

            if (Instance != null)
                acc += Instance.GetHashCode();

            return acc;
        }

        public override bool Equals(object obj)
        {
            var o = obj as PropertySetTarget;
            if (o == null)
                return false;

            return Concept == o.Concept && Instance == o.Instance && Property == o.Property;
        }
    }
}
