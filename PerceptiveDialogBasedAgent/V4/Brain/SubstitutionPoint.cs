using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    class SubstitutionPoint
    {
        internal readonly PlanProvider Owner;

        internal readonly MindState State;

        internal readonly ConceptInstance Pattern;

        internal readonly double SubstitutionScore;

        public SubstitutionPoint(ActionManagerPlanProvider actionRequester, ConceptInstance pattern, MindState mindState, double substitutionScore)
        {
            Owner = actionRequester;
            Pattern = pattern;
            State = mindState;
            SubstitutionScore = substitutionScore;
        }

        internal MindState Substitute(PointableInstance instance)
        {
            if (!State.PropertyContainer.MeetsPattern(instance, Pattern))
                return null;

            var substitutedState = Owner.Substitute(instance, State);
            return substitutedState.AddScore(SubstitutionScore);
        }

    }
}
