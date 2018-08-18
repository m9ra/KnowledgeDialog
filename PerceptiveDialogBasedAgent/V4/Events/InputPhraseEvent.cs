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

        internal readonly int InputId;

        private static int _lastId = 0;

        public InputPhraseEvent(string phrase)
        {
            Phrase = phrase;
            InputId = ++_lastId;
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
