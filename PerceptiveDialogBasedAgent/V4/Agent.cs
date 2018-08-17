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
            foreach (var word in new[] { "in", "a", "an", "the", "it", "no", "yes", "is", "find", "search", "google", "lookup", "look", "want", "get", "give", "expensive", "cheap", "stupid", "what", "where", "which", "name" })
            {
                _irrelevantWords.Remove(word);
            }
            Reset();
        }

        internal void Reset()
        {
            _beam = new ComposedPolicyBeamGenerator();

            _beam.RegisterAbility(new OptionPrompt());
            _beam.RegisterAbility(new AcceptNewProperty());
            _beam.RegisterAbility(new RememberPropertyValue());
            _beam.RegisterAbility(new DoYouKnow());
            _beam.RegisterAbility(new PartialDoYouKnow());
            _beam.RegisterAbility(new EssentialKnowledge());
            _beam.RegisterAbility(new RestaurantDomainKnowledge());
            _beam.RegisterAbility(new ItReferenceResolver());
            _beam.RegisterAbility(new DefiniteReferenceResolver());
            _beam.RegisterAbility(new FindProvider());
            _beam.RegisterAbility(new WhatProvider());
            _beam.RegisterAbility(new YesNoPrompt());
            _beam.RegisterAbility(new AssignUnknownProperty());
            _beam.RegisterAbility(new PropertyValueDisambiguation());
            _beam.RegisterAbility(new RememberConceptDescription());
            _beam.RegisterAbility(new CollectNewConceptLearning());

            //NOTE: Ordering of policy parts matters
            _beam.AddPolicyPart(new HowCanIHelpYouFallback());
            _beam.AddPolicyPart(new RequestActionWithKnownConfirmation());
            _beam.AddPolicyPart(new AfterDescriptionRemembered());

            _beam.AddPolicyPart(new LearnNewPhrase());
            _beam.AddPolicyPart(new UnknownAnsweredToLearnNewPhrase());
            _beam.AddPolicyPart(new RememberDescriptionAfterLearnNewPhrase());
            _beam.AddPolicyPart(new LearnPropertyValue());
            _beam.AddPolicyPart(new AfterPropertyLearned());

            _beam.AddPolicyPart(new UnknownAnsweredToRefinement());

            _beam.AddPolicyPart(new RequestSubstitution());
            _beam.AddPolicyPart(new RequestSubstitutionWithUnknown());

            _beam.AddPolicyPart(new RequestNewPropertyExplanation());
            _beam.AddPolicyPart(new ReaskAssignUnknownValue());

            _beam.AddPolicyPart(new AskForDisambiguation());
            _beam.AddPolicyPart(new ReaskDisambiguation());

            _beam.AddPolicyPart(new OfferResult());
            _beam.AddPolicyPart(new ProcessCollectedNewConceptLearning());
            _beam.AddPolicyPart(new RequestRefinement());
            _beam.AddPolicyPart(new LearnUnknownForRefinement());
        }

        internal void AcceptKnowledge(EventBase evt)
        {
            _beam.PushToAll(evt);
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
