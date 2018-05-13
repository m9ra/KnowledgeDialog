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
    }
}
