using PerceptiveDialogBasedAgent.V2;
using PerceptiveDialogBasedAgent.V4.Abilities;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Models;
using PerceptiveDialogBasedAgent.V4.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Agent
    {
        private ComposedPolicyBeamGenerator _beam;

        private readonly HashSet<string> _irrelevantWords = new HashSet<string>();

        internal string LastOutput { get; private set; }

        internal BeamNode LastBestNode { get; private set; }

        internal Agent()
        {
            _irrelevantWords.UnionWith(KnowledgeDialog.Dialog.UtteranceParser.NonInformativeWords);
            foreach (var word in new[] { "a", "an", "the", "it", "no", "yes", "is", "find", "search", "google", "lookup", "look", "want", "get", "give", "expensive", "cheap", "stupid", "what", "where", "which", "name" })
            {
                _irrelevantWords.Remove(word);
            }
            Reset();
        }

        internal void Reset()
        {
            _beam = new ComposedPolicyBeamGenerator();

            _beam.RegisterAbility(new RememberPropertyValue());
            _beam.RegisterAbility(new DoYouKnow());
            _beam.RegisterAbility(new PartialDoYouKnow());
            _beam.RegisterAbility(new EssentialKnowledge());
            _beam.RegisterAbility(new RestaurantDomainKnowledge());
            _beam.RegisterAbility(new ItReferenceResolver());
            _beam.RegisterAbility(new DefiniteReferenceResolver());
            _beam.RegisterAbility(new FindProvider());
            _beam.RegisterAbility(new WhatProvider());
            _beam.RegisterAbility(new PromptAbility());
            _beam.RegisterAbility(new AssignUnknownProperty());
            _beam.RegisterAbility(new PropertyValueDisambiguation());

            //NOTE: Ordering of policy parts matters
            _beam.AddPolicyPart(new HowCanIHelpYouFallback());
            _beam.AddPolicyPart(new RequestActionWithKnownConfirmation());
            _beam.AddPolicyPart(new LearnPropertyValue());
            _beam.AddPolicyPart(new RequestSubstitution());
            _beam.AddPolicyPart(new AssignUnknownValue());

            _beam.AddPolicyPart(new AskForDisambiguation());
            _beam.AddPolicyPart(new ReaskDisambiguation());

            _beam.AddPolicyPart(new OfferResult());
            _beam.AddPolicyPart(new RequestRefinement());
            _beam.AddPolicyPart(new LearnUnknownForRefinement());


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
                Log.States(_beam, 1);

                _beam.LimitBeam(1);

                var nlg = new EventBasedNLG();
                response = nlg.GenerateResponse(_beam.GetBestNode());
            }

            Log.DialogUtterance("S: " + response);
            LastOutput = response;
            LastBestNode = _beam.GetBestNode();

            return response;
        }
    }
}
