using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3.Models
{
    class SimpleMatchModel : ScoreModelBase
    {
        internal override double ReadoutScore(BodyState s)
        {
            //var score = s.Score - s.PendingRequirements.Count();
            var score = s.Score;
            return score;
        }

        internal override double ParameterAssignScore(BodyState state, ConceptRequirement parameter, Concept concept)
        {
            //we don't have model for parameter assign
            return 1.0;
        }

        internal override double PointingScore(BodyState state, InputPhrase phrase, Concept concept)
        {
            var phraseStr = phrase.ToString();
            if (phraseStr == concept.Name)
                return 1.0;

            var totalDescriptionLength = 0;
            var hitLength = 0;

            foreach (var description in concept.Descriptions)
            {
                totalDescriptionLength += description.Length;
                hitLength = description.Contains(phraseStr) ? phraseStr.Length : 0;
            }

            var score = 1.0 * hitLength / (totalDescriptionLength + 1);
            return score;
        }
    }
}
