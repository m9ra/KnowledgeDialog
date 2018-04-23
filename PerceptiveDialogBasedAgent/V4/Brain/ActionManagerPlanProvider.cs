using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    class ActionManagerPlanProvider : PlanProvider
    {
        private readonly Body _body;

        internal readonly MindField<IEnumerable<ConceptInstance>> CompleteActions = new MindField<IEnumerable<ConceptInstance>>(nameof(CompleteActions));

        internal ActionManagerPlanProvider(Body body)
        {
            _body = body;
        }

        internal override MindState GenerateQuestion(MindState bestMindState)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<SubstitutionPoint> GenerateSubstitutionPoints(MindState mindState, double providerWeight)
        {
            var pattern = new ConceptInstance(Concept2.Something);
            mindState = mindState.SetPropertyValue(pattern, Concept2.NativeAction, Concept2.Something);
            yield return new SubstitutionPoint(this, pattern, mindState, providerWeight);
        }

        internal override MindState Substitute(PointableInstance instance, MindState state)
        {
            var conceptInstance = instance as ConceptInstance;
            if (conceptInstance == null)
                throw new NotImplementedException();

            var activatedPlanProviders = new List<PlanProvider>();
            foreach (var property in state.GetProperties(conceptInstance))
            {
                if (!state.PropertyContainer.IsParameter(property))
                    continue;

                var originalValue = conceptInstance.Concept.GetPropertyValue(property);
                var currentValue = state.GetPropertyValue(conceptInstance, property);
                var isSubstituted = originalValue != currentValue;
                if (!isSubstituted)
                    throw new NotImplementedException();
            }

            if (activatedPlanProviders.Any())
            {
                throw new NotImplementedException();
            }

            //instance was fully substituted, lets let it talk to the state
            var context = new MindEvaluationContext(conceptInstance, state);
            state = context.Evaluate();
            return state.SetValue(CompleteActions, new[] { conceptInstance });
        }
    }
}
