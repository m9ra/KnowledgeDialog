using PerceptiveDialogBasedAgent.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3.Models
{
    abstract class ScoreModelBase
    {
        internal abstract double ReadoutScore(BodyState state);

        internal abstract double ParameterAssignScore(BodyState state, ConceptRequirement parameter, Concept concept);

        internal abstract double PointingScore(BodyState state, Phrase phrase, Concept concept);
    }
}
