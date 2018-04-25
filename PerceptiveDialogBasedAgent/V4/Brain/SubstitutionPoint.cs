using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    class SubstitutionPoint
    {
        internal readonly ConceptInstance Target;

        internal readonly Concept2 TargetProperty;

        internal readonly MindState State;

        internal readonly ConceptInstance Pattern;

        internal readonly double SubstitutionScore;

        public SubstitutionPoint(ConceptInstance target, Concept2 targetProperty, ConceptInstance pattern, MindState mindState, double substitutionScore)
        {
            Target = target;
            TargetProperty = targetProperty;
            Pattern = pattern;
            State = mindState;
            SubstitutionScore = substitutionScore;
        }

        internal MindState Substitute(PointableInstance instance)
        {
            if (!State.PropertyContainer.MeetsPattern(instance, Pattern))
                return null;

            var newState = State.SetPropertyValue(Target, TargetProperty, instance);
            if (newState.GetAvailableParameters(Target).Any())
                return newState;

            //Target substitution is complete - lets call it (TODO resolve child/parent call deps)
            var context = new MindEvaluationContext(Target, newState);
            var resultState = context.EvaluateOnParametersComplete();
            return resultState.AddScore(SubstitutionScore);
        }

    }
}
