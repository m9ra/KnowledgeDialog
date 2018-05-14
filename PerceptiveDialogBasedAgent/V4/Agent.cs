using PerceptiveDialogBasedAgent.V2;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Agent
    {
        private readonly RestaurantDomainBeamGenerator _beam;

        internal Agent()
        {
            _beam = new RestaurantDomainBeamGenerator();
        }

        internal string Input(string originalSentence)
        {
            Log.DialogUtterance("U: " + originalSentence);

            var words = Phrase.AsWords(originalSentence);
            if (words.Length > 10)
                return "I'm sorry, the sentence is too long. Try to use simpler sentences please.";

            _beam.PushToAll(new TurnStartEvent());
            foreach (var word in words)
            {
                _beam.LimitBeam(500);
                _beam.PushInput(word);
            }

            _beam.PushToAll(new TurnEndEvent());
            _beam.LimitBeam(10);
            Log.States(_beam, 1);

            var nlg = new EventBasedNLG();
            var response = nlg.GenerateResponse(_beam.GetBestNode());

            Log.DialogUtterance("S: " + response);
            return response;
        }
    }
}
