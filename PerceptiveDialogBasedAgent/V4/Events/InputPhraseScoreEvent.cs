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
        internal readonly InputPhraseEvent InputPhrase
            ;
        internal readonly Concept2 Concept;

        public InputPhraseScoreEvent(InputPhraseEvent inputPhrase, Concept2 concept)
        {
            InputPhrase = inputPhrase;
            Concept = concept;
        }

        internal override double GetDefaultScore()
        {
            return getSimilarity(InputPhrase.Phrase, Concept.Name, new string[0]); //TODO solve the descriptions
        }

        private double getSimilarity(string input, string conceptName, IEnumerable<string> conceptDescriptions)
        {
            var sanitizedInput = input.ToLowerInvariant();
            var meaningFulInput = HandcraftedModel.ToMeaningfulPhrase(input);
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
                if (HandcraftedModel.AuxiliaryWords.Contains(word))
                    continue;

                var hitCount = 0.0;
                var totalWeight = 0.0;
                foreach (var description in conceptDescriptions.Concat(new[] { name }))
                {
                    var descriptionWords = Phrase.AsWords(description.ToLowerInvariant());
                    foreach (var descriptionWord in descriptionWords)
                    {
                        if (HandcraftedModel.AuxiliaryWords.Contains(descriptionWord))
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
    }
}
