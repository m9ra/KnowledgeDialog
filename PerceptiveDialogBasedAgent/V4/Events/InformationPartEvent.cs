using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class InformationPartEvent : EventBase
    {
        public readonly ConceptInstance Subject;

        public readonly Concept2 Property;

        public readonly ConceptInstance Value;

        public bool IsFilled => Subject != null && Value != null && Property != null;

        public InformationPartEvent(ConceptInstance subject, Concept2 property, ConceptInstance value)
        {
            Subject = subject;
            Property = property;
            Value = value;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        internal InformationPartEvent SubstituteValue(ConceptInstance value)
        {
            return new InformationPartEvent(Subject, Property, value);
        }

        internal InformationPartEvent SubstituteSubject(ConceptInstance subject)
        {
            return new InformationPartEvent(subject, Property, Value);
        }

        public override string ToString()
        {
            return $"[part: {represent(Subject)}<--{represent(Property)}--{represent(Value)}]";
        }

        private string represent(ConceptInstance subject)
        {
            return represent(subject?.Concept);
        }

        private string represent(Concept2 concept)
        {
            if (concept == null)
                return "$";

            return concept.Name;
        }
    }
}
