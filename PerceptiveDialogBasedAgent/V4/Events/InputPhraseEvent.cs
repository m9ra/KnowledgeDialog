using PerceptiveDialogBasedAgent.V4.EventBeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class InputPhraseEvent : EventBase
    {
        internal readonly string Phrase;

        public InputPhraseEvent(string phrase)
        {
            Phrase = phrase;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[\"{Phrase}\"]";
        }
    }
}
