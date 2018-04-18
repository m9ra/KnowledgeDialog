using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class ConceptInstance : PointableInstance
    {
        internal readonly Concept2 Concept;

        internal ConceptInstance(Concept2 concept, Phrase activationPhrase = null)
            : base(activationPhrase)
        {
            Concept = concept;
        }

        internal override IEnumerable<PointableInstance> GetPropertyValue(Concept2 property)
        {
            return Concept.GetPropertyValue(property);
        }

        internal override string ToPrintable()
        {
            return Concept.Name;
        }

        public override string ToString()
        {
            return $"[{Concept}]";
        }
    }
}
