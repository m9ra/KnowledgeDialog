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
        private RestaurantDomainBeamGenerator _beam;

        private readonly HashSet<string> _irrelevantWords = new HashSet<string>();

        internal Agent()
        {
            _irrelevantWords.UnionWith(KnowledgeDialog.Dialog.UtteranceParser.NonInformativeWords);
            foreach (var word in new[] { "find", "search", "google", "lookup", "look", "want", "get", "give", "expensive", "cheap", "stupid", "what", "where", "which" })
            {
                _irrelevantWords.Remove(word);
            }
            Reset();
        }

        internal void Reset()
        {
            _beam = new RestaurantDomainBeamGenerator();
        }

        internal string Input(string originalSentence)
        {
            Log.DialogUtterance("U: " + originalSentence);

            var words = Phrase.AsWords(originalSentence.ToLower());
            string response;
            if (words.Length > 10)
            {
                response = "I'm sorry, the sentence is too long. Try to use simpler sentences please.";
            }
            else
            {
                _beam.PushToAll(new TurnStartEvent());
                foreach (var word in words)
                {
                    if (_irrelevantWords.Contains(word))
                        //TODO for computational efficiency skip non important words
                        continue;

                    _beam.LimitBeam(500);
                    _beam.PushInput(word);
                }

                _beam.PushToAll(new TurnEndEvent());
                _beam.LimitBeam(10);
                Log.States(_beam, 1);

                var nlg = new EventBasedNLG();
                response = nlg.GenerateResponse(_beam.GetBestNode());
            }

            Log.DialogUtterance("S: " + response);
            return response;
        }
    }
}
