using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Models
{
    class HandcraftedModel : PointingGeneratorBase
    {
        private readonly Body _body;

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
            if (state.LastInputPhrase == null)
                yield break;

            foreach (var conceptMatch in generateConceptMatches(state))
                yield return conceptMatch;
        }

        private IEnumerable<RankedPointing> generateConceptMatches(BodyState2 state)
        {
            var input = state.LastInputPhrase.ToString();
            foreach (var concept in _body.Concepts)
            {
                var similarity = getSimilarity(input, concept);
                if (similarity > 0)
                    yield return new RankedPointing(state.LastInputPhrase, new ConceptInstance(concept), similarity);
            }
        }

        private double getSimilarity(string input, Concept2 concept)
        {
            if (input == concept.Name)
                return 1.0;

            var totalDescriptionLength = 0;
            var hitLength = 0;

            foreach (var description in concept.Descriptions)
            {
                totalDescriptionLength += description.Length;
                hitLength = description.Contains(input) ? input.Length : 0;
            }

            var score = 1.0 * hitLength / (totalDescriptionLength + 1);
            return score;
        }
    }
}
