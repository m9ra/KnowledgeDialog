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

        private readonly Random _rnd = new Random();

        private DocumentIndex _index = new DocumentIndex();

        private readonly HashSet<string> _auxiliaryWords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "a", "an", "the", "on", "at", "in", "of", "some", "any", "none", "such", "to", "and" };

        private string _previousUnknownPhrase;

        private string _lastUnsuccesfulInput;

        private BodyState2 _lastUnsuccesfulState;

        internal HandcraftedModel(Body body)
        {
            _body = body;
        }

        internal override BodyState2 AddSubstitution(BodyState2 state, ConceptParameter parameter, ConceptInstance value)
        {
            if (value == null)
                throw new NullReferenceException();

            return state.AddSubstitution(parameter, value, 0.1);
        }

        internal override IEnumerable<RankedPointing> GenerateMappings(BodyState2 state)
        {
            return findLastPhraseConcepts(state);
        }

        internal override IEnumerable<RankedPointing> GetForwardings(ConceptInstance forwardedInstance, BodyState2 state)
        {
            return findDescriptionSimilarConcepts(forwardedInstance, state);
        }

        internal override void OnConceptChange()
        {
            refreshIndex();
        }

        internal override BodyState2 StateReaction(BodyState2 state)
        {
            LogState(state);

            var output = limitedStateReaction(ref state, 0);
            if (output == null)
            {
                output = makeUpExplorativeQuestion(ref state);
            }

            Log.DialogUtterance("S: " + output);
            return state;
        }

        private string limitedStateReaction(ref BodyState2 state, int depth)
        {
            string output = null;
            if (hasAskedForUnknownPhrase())
            {
                rememberNewConcept(_previousUnknownPhrase, _body.CurrentInput);
                output = retryUnsuccessfulState(out state, depth);
            }
            else if (hasOutput(state))
            {
                output = readOutput(state);
            }
            else if (hasSingleUnknownPhrase(state))
            {
                output = askForUnknownPhrase(state);
            }
            else if (needsParameter(state))
            {
                output = askForParameter(state);
            }

            return output;
        }

        private string makeUpExplorativeQuestion(ref BodyState2 state)
        {
            //ask for some unknown phrase
            var unknownPhrases = getUnknownPhrases(state);
            if (unknownPhrases.Any())
                askForUnknownPhrase(unknownPhrases.First());

            //ask for info about non-native concept
            var concepts = getNonNativeConcepts().ToArray();
            if (concepts.Any())
                askForUnknownPhrase(concepts[_rnd.Next(concepts.Length)].Name);

            state = BodyState2.Empty();
            return "Sorry, I don't understand. How can I help you?";
        }


        #region Policy implementation

        private bool hasOutput(BodyState2 state)
        {
            return readOutput(state) != null;
        }

        private bool hasSingleUnknownPhrase(BodyState2 state)
        {
            return getUnknownPhrases(state).Count() == 1;
        }

        private bool needsParameter(BodyState2 state)
        {
            return askForParameter(state) != null;
        }

        private string askForUnknownPhrase(BodyState2 state)
        {
            var unknownPhrase = getUnknownPhrases(state).FirstOrDefault();
            if (unknownPhrase == null)
                return null;

            return askForUnknownPhrase(unknownPhrase);
        }

        private string askForUnknownPhrase(string unknownPhrase)
        {
            _previousUnknownPhrase = unknownPhrase ?? throw new NotImplementedException("What should agent do?");
            _lastUnsuccesfulState = _body.LastFinishedState;
            _lastUnsuccesfulInput = _body.CurrentInput;

            return $"What is {unknownPhrase} ?";
        }

        private string askForParameter(BodyState2 state)
        {
            var parameter = state.AvailableParameters.LastOrDefault();
            if (parameter == null)
                return null;

            return parameter.Request;
        }

        private string retryUnsuccessfulState(out BodyState2 state, int depth)
        {
            state = _body.InputTransition(new[] { _lastUnsuccesfulState }, _lastUnsuccesfulInput);

            var retryOutput = readOutput(state);
            if (depth < 1)
                retryOutput = limitedStateReaction(ref state, depth + 1);

            if (retryOutput == null)
                return null;

            var continuationPrefix = choose(new[]
            {
                "So", "Well", "Ok", "Hm", "In that case"
            });

            var output = continuationPrefix + ", " + retryOutput;
            Log.Indent();
            Log.Writeln("RETRY: " + _lastUnsuccesfulInput, Log.PolicyColor);
            LogState(state);
            Log.Dedent();

            return output;
        }

        private void rememberNewConcept(string concept, string description)
        {
            _body.Concept(_previousUnknownPhrase, null, isNative: false).Description(_body.CurrentInput); //TODO more complex parsing could take place here
            _previousUnknownPhrase = null;
        }

        private bool hasAskedForUnknownPhrase()
        {
            return _previousUnknownPhrase != null;
        }

        private string readOutput(BodyState2 state)
        {
            var outputValue = state.GetIndexValue(_body.CurrentAgentInstance, _body.OutputProperty);

            return outputValue?.ToPrintable();
        }

        private IEnumerable<Concept2> getNonNativeConcepts()
        {
            return _body.Concepts.Where(c => !c.IsNative).ToArray();
        }

        private IEnumerable<string> getUnknownPhrases(BodyState2 state)
        {
            foreach (var inputPhrase in state.InputPhrases.Reverse())
            {
                var sanitizedPhrase = toMeaningfulPhrase(inputPhrase.ToPrintable());
                if (sanitizedPhrase == "")
                    continue;

                var instance = state.GetRankedPointing(inputPhrase);
                if (instance == null)
                    yield return sanitizedPhrase;
            }
        }

        private T choose<T>(IEnumerable<T> values)
        {
            var count = values.Count();
            return values.Skip(_rnd.Next(count)).First();
        }

        #endregion

        #region Probabilistic model implementation

        private IEnumerable<RankedPointing> findLastPhraseConcepts(BodyState2 state)
        {
            if (state.LastInputPhrase == null)
                yield break;

            foreach (var conceptMatch in generateConceptMatches(state))
                yield return conceptMatch;
        }

        private IEnumerable<RankedPointing> findDescriptionSimilarConcepts(ConceptInstance forwardedInstance, BodyState2 state)
        {
            foreach (var forwardMatch in generateForwardMatches(state, forwardedInstance))
                yield return forwardMatch;
        }

        private IEnumerable<RankedPointing> generateForwardMatches(BodyState2 state, ConceptInstance forwardedConcept)
        {
            foreach (var concept in _body.Concepts)
            {
                if (concept == forwardedConcept.Concept)
                    //disable self referencing
                    continue;

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
            var sanitizedInput = input.ToLowerInvariant();
            var meaningFulInput = toMeaningfulPhrase(input);
            var words = sanitizedInput.Split(' ');
            var name = concept.Name.ToLowerInvariant();
            var weight = 1.0 * words.Length;
            weight = 1 + words.Length / 100.0;
            if (sanitizedInput == name || meaningFulInput == name)
                return 1.0 * words.Length * weight;

            foreach (var description in concept.Descriptions)
            {
                if (description.ToLowerInvariant() == sanitizedInput)
                    return 0.9 * words.Length * weight;
            }

            var scores = new List<double>();
            foreach (var word in words)
            {
                if (_auxiliaryWords.Contains(word))
                    continue;

                var hitCount = 0.0;
                var totalWeight = 0.0;
                foreach (var description in concept.Descriptions.Concat(new[] { name }))
                {
                    var descriptionWords = description.ToLowerInvariant().Split(' ');
                    foreach (var descriptionWord in descriptionWords)
                    {
                        if (_auxiliaryWords.Contains(descriptionWord))
                            continue;

                        var wordWeight = 1.0 / _index.TotalOccurences(descriptionWord);
                        totalWeight += wordWeight;
                        if (descriptionWord == word)
                            hitCount += wordWeight;
                    }
                }
                var wordScore = 1.0 * hitCount / (totalWeight + 1);
                scores.Add(wordScore);
            }

            if (scores.Count == 0)
                return 0;

            var score = scores.Sum() * weight;
            return score;
        }

        private string toMeaningfulPhrase(string phrase)
        {
            var input = phrase.ToLowerInvariant();
            var inputWords = input.Split(' ').ToList();

            while (inputWords.Count > 0 && _auxiliaryWords.Contains(inputWords.First()))
                inputWords.RemoveAt(0);

            while (inputWords.Count > 0 && _auxiliaryWords.Contains(inputWords.Last()))
                inputWords.RemoveAt(inputWords.Count - 1);

            return string.Join(" ", inputWords);
        }

        private double getSimilarity2(string input, Concept2 concept)
        {
            var sanitizedInput = input.ToLowerInvariant();
            var words = sanitizedInput.Split(' ');
            var name = concept.Name.ToLowerInvariant();
            var weight = 1.0 * words.Length;
            weight = 1 + words.Length / 100.0;
            if (input == name)
                return 1.0 * words.Length * weight;

            foreach (var description in concept.Descriptions)
            {
                if (description.ToLowerInvariant() == sanitizedInput)
                    return 0.9 * words.Length * weight;
            }

            var scores = new List<double>();
            foreach (var description in concept.Descriptions.Concat(new[] { name }))
            {
                var descriptionWords = description.ToLowerInvariant().Split(' ');

                var hitCount = 0.0;
                var totalWeight = 0.0;
                foreach (var descriptionWord in descriptionWords)
                {
                    if (_auxiliaryWords.Contains(descriptionWord))
                        continue;

                    var wordWeight = 1.0 / _index.TotalOccurences(descriptionWord);
                    totalWeight += wordWeight;

                    var wordScores = new List<double>();
                    foreach (var word in words)
                    {
                        if (_auxiliaryWords.Contains(word))
                            continue;

                        if (descriptionWord == word)
                            wordScores.Add(wordWeight);
                    }

                    if (wordScores.Count > 0)
                        hitCount += wordScores.Max();
                }

                var descriptionScore = 1.0 * hitCount / (totalWeight + 1);
                scores.Add(descriptionScore);
            }

            if (scores.Count == 0)
                return 0;

            var score = scores.Max() * weight;
            return score;
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

        #endregion

    }
}
