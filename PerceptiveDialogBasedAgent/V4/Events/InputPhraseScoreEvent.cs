using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Events
{
    class InputPhraseScoreEvent : TracedScoreEventBase
    {
        internal static readonly HashSet<string> AuxiliaryWords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "a", "an", "the", "on", "at", "in", "of", "some", "any", "none", "such", "to", "and", "with" };

        internal readonly InputPhraseEvent InputPhrase;

        internal readonly Concept2 Concept;

        public InputPhraseScoreEvent(InputPhraseEvent inputPhrase, Concept2 concept)
        {
            InputPhrase = inputPhrase;
            Concept = concept;
        }

        internal override double GetDefaultScore(BeamNode node)
        {
            var descriptions = BeamGenerator.GetDescriptions(Concept, node);
            return getSimilarity(InputPhrase.Phrase, Concept.Name, descriptions); //TODO solve the descriptions
        }

        internal static string ToMeaningfulPhrase(string phrase)
        {
            var input = phrase.ToLowerInvariant();
            var inputWords = Phrase.AsWords(input).ToList();

            while (inputWords.Count > 0 && AuxiliaryWords.Contains(inputWords.First()))
                inputWords.RemoveAt(0);

            while (inputWords.Count > 0 && AuxiliaryWords.Contains(inputWords.Last()))
                inputWords.RemoveAt(inputWords.Count - 1);

            return string.Join(" ", inputWords);
        }

        private double getSimilarity(string input, string conceptName, IEnumerable<string> conceptDescriptions)
        {
            var sanitizedInput = input.ToLowerInvariant();
            var meaningFulInput = ToMeaningfulPhrase(input);
            var words = Phrase.AsWords(sanitizedInput);
            var name = conceptName.ToLowerInvariant();
            var weight = 1.0 * words.Length;
            weight = 1 + words.Length / 100.0;
            if (sanitizedInput == name || meaningFulInput == name)
                return 1.0 * words.Length * weight;

            foreach (var description in conceptDescriptions)
            {
                if (description.ToLowerInvariant() == sanitizedInput)
                    return 0.9 * words.Length * weight;
            }

            var scores = new List<double>();
            foreach (var word in words)
            {
                if (AuxiliaryWords.Contains(word))
                    continue;

                var hitCount = 0.0;
                var totalWeight = 0.0;
                foreach (var description in conceptDescriptions.Concat(new[] { name }))
                {
                    var descriptionWords = Phrase.AsWords(description.ToLowerInvariant());
                    foreach (var descriptionWord in descriptionWords)
                    {
                        if (AuxiliaryWords.Contains(descriptionWord))
                            continue;

                        //var wordWeight = 1.0 / _index.TotalOccurences(descriptionWord);
                        var wordWeight = 1.0; //TODO better weighing should be here
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

        internal override IEnumerable<string> GenerateFeatures(BeamNode node)
        {
            yield break;
        }
    }
}
