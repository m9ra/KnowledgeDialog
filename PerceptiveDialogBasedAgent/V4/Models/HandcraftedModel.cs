using KnowledgeDialog.Knowledge;
using PerceptiveDialogBasedAgent.V2;
using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Models
{
    class HandcraftedModel : PointingModelBase
    {
        private readonly Body _body;

        private DocumentIndex _index = new DocumentIndex();

        private string _previousUnknownPhrase;

        private string _lastUnsuccesfulInput;

        private BodyState2 _lastUnsuccesfulState;

        internal HandcraftedModel(Body body)
        {
            _body = body;
        }

        internal override BodyState2 StateReaction(BodyState2 state)
        {
            logState(state);


            string output = null;
            if (_previousUnknownPhrase != null)
            {
                _body.Concept(_previousUnknownPhrase, null, isNative: false).Description(_body.CurrentInput); //TODO more complex parsing could take place here
                state = _body.InputTransition(new[] { _lastUnsuccesfulState }, _lastUnsuccesfulInput);
                output = "Ok, so I assume I should respond by " + getOutput(state);
                Log.Indent();
                Log.Writeln("RETRY: " + _lastUnsuccesfulInput, Log.PolicyColor);
                logState(state);
                Log.Dedent();
            }
            else
            {
                output = getOutput(state);
            }

            Log.DialogUtterance("S: " + output);
            return state;
        }

        private void logState(BodyState2 state)
        {
            Log.Indent();
            foreach (var input in state.InputPhrases)
            {
                Log.Writeln(input.ToString(), Log.SensorColor);
                Log.Indent();
                var inputTarget = getTargetRepresentation(input, state);

                Log.Writeln(inputTarget, Log.ItemColor);
                Log.Dedent();
            }
            Log.Dedent();
            Log.Writeln();
        }

        private string getTargetRepresentation(PointableBase source, BodyState2 state)
        {
            var rankedPointing = state.GetRankedPointing(source);
            if (rankedPointing == null)
                return "unknown";

            var strRepresentation = rankedPointing.Target.ToString() + $"~{rankedPointing.Rank:0.00}";
            var forwardedPointing = state.GetRankedPointing(rankedPointing.Target);
            if (forwardedPointing != null)
                strRepresentation += " --> " + getTargetRepresentation(rankedPointing.Target, state);

            return strRepresentation;
        }


        private string getOutput(BodyState2 state)
        {
            var outputValue = state.GetIndexValue(_body.CurrentAgentInstance, _body.OutputProperty);
            if (outputValue == null)
            {
                var unknownPhrase = getUnknownPhrase(state);
                _previousUnknownPhrase = unknownPhrase ?? throw new NotImplementedException("What should agent do?");
                _lastUnsuccesfulState = _body.LastFinishedState;
                _lastUnsuccesfulInput = _body.CurrentInput;

                return $"What is {unknownPhrase} ?";
            }
            return outputValue.Concept.Name;
        }

        private string getUnknownPhrase(BodyState2 state)
        {
            foreach (var inputPhrase in state.InputPhrases.Reverse())
            {
                var instance = state.GetRankedPointing(inputPhrase);
                if (instance == null)
                    return inputPhrase.ToString();
            }

            return null;
        }

        internal override BodyState2 AddSubstitution(BodyState2 state, ConceptParameter parameter, ConceptInstance value)
        {
            if (value == null)
                throw new NullReferenceException();

            return state.AddSubstitution(parameter, value, 0.1);
        }

        internal override IEnumerable<RankedPointing> GenerateMappings(BodyState2 state)
        {
            if (state.LastInputPhrase == null)
                yield break;

            foreach (var conceptMatch in generateConceptMatches(state))
                yield return conceptMatch;
        }

        internal override IEnumerable<RankedPointing> GetForwardings(ConceptInstance forwardedConcept, BodyState2 state)
        {
            foreach (var forwardMatch in generateForwardMatches(state, forwardedConcept))
                yield return forwardMatch;
        }

        private IEnumerable<RankedPointing> generateForwardMatches(BodyState2 state, ConceptInstance forwardedConcept)
        {
            foreach (var concept in _body.Concepts)
            {
                var similarities = new List<double>();
                foreach (var forwardedDescription in forwardedConcept.Concept.Descriptions)
                {
                    var similarity = getSimilarity(forwardedDescription, concept);
                    similarities.Add(similarity);
                }

                var forwardingSimilarity = similarities.Max();
                if (forwardingSimilarity > 0.05)
                    //todo should new concept be created here?
                    yield return new RankedPointing(forwardedConcept, new ConceptInstance(concept), forwardingSimilarity);
            }
        }

        private IEnumerable<RankedPointing> generateConceptMatches(BodyState2 state)
        {
            var input = state.LastInputPhrase.ToString();
            foreach (var concept in _body.Concepts)
            {
                var similarity = getSimilarity(input, concept);
                if (similarity > 0.05)
                    yield return new RankedPointing(state.LastInputPhrase, new ConceptInstance(concept), similarity);
            }
        }

        private double getSimilarity(string input, Concept2 concept)
        {
            var words = input.Split(' ');
            var weight = 1.0 * words.Length;
            if (input == concept.Name)
                return 1.0 * weight;

            var scores = new List<double>();
            foreach (var word in words)
            {
                var hitCount = 0.0;
                var descriptionLength = 0.0;

                foreach (var description in concept.Descriptions.Concat(new[] { concept.Name }))
                {
                    var descriptionWords = description.Split(' ');
                    foreach (var descriptionWord in descriptionWords)
                    {
                        var wordWeight = 1.0 / _index.TotalOccurences(descriptionWord);
                        descriptionLength += wordWeight;
                        if (descriptionWord.ToLowerInvariant() == word.ToLowerInvariant())
                            hitCount += wordWeight;
                    }
                }
                var wordScore = 1.0 * hitCount / (descriptionLength + 1);
                scores.Add(wordScore);
            }

            var score = scores.Max() * weight;
            return score;
        }

        internal override void OnConceptChange()
        {
            refreshIndex();
        }

        private void refreshIndex()
        {
            _index = new DocumentIndex();

            foreach (var concept in _body.Concepts)
            {
                var conceptSentences = concept.Descriptions.ToList();
                conceptSentences.Add(concept.Name);

                var conceptWords = conceptSentences.Select(s => s.ToLowerInvariant().Split(' '));
                _index.RegisterDocument(conceptWords);
            }
        }
    }
}
