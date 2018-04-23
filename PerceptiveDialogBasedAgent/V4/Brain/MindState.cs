using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    class MindState
    {
        private readonly Dictionary<MindField, object> _stateValues;

        private readonly List<PlanProvider> _planProviders;

        internal readonly PropertyContainer PropertyContainer;

        internal readonly double Score;

        private MindState(MindState previousState, double extraScore = 0, Dictionary<MindField, object> stateValues = null, List<PlanProvider> planProviders = null, PropertyContainer container = null)
        {
            var score = previousState?.Score ?? 0;

            Score = score + extraScore;
            PropertyContainer = container ?? previousState.PropertyContainer;
            _stateValues = stateValues ?? previousState._stateValues;
            _planProviders = planProviders ?? previousState._planProviders;
        }

        internal MindState AddPlanProvider(PlanProvider planProvider)
        {
            var newPlanProviders = new List<PlanProvider>(_planProviders);
            newPlanProviders.Add(planProvider);

            return new MindState(this, planProviders: newPlanProviders);
        }

        internal MindState AddScore(double extraScore)
        {
            return new MindState(this, extraScore: extraScore);
        }

        internal PointableInstance GetPropertyValue(PointableInstance instance, Concept2 parameter)
        {
            return PropertyContainer.GetPropertyValue(instance, parameter);
        }

        internal MindState SetPropertyValue(PointableInstance instance, Concept2 parameter, Concept2 value)
        {
            var newContainer = PropertyContainer.SetPropertyValue(instance, parameter, value);
            return new MindState(this, container: newContainer);
        }

        internal MindState Import(PointableInstance instance, PropertyContainer container)
        {
            var newContainer = PropertyContainer.Import(instance, container);
            return new MindState(this, container: newContainer);
        }

        internal IEnumerable<Concept2> GetProperties(PointableInstance instance)
        {
            return PropertyContainer.GetProperties(instance);
        }

        internal static MindState Empty()
        {
            return new MindState(null, extraScore: 0, stateValues: new Dictionary<MindField, object>(), planProviders: new List<PlanProvider>(), container: new PropertyContainer());
        }

        internal MindState SetValue<T>(MindField field, T value)
        {
            var newValues = new Dictionary<MindField, object>(_stateValues);
            newValues[field] = value;

            return new MindState(this, stateValues: newValues);
        }

        internal T GetValue<T>(MindField<T> field)
        {
            _stateValues.TryGetValue(field, out var value);
            return (T)value;
        }

        internal IEnumerable<SubstitutionPoint> GetSubstitutionPoints()
        {
            //TODO substition points for all the providers with decaying weight should be here.
            var providerWeight = 1.0; //reflects actuality of the topic
            return _planProviders.Last().GenerateSubstitutionPoints(this, providerWeight);
        }

        internal PlanProvider GetActivePlanProvider()
        {
            return _planProviders.LastOrDefault();
        }
    }

    class MindField
    {
        internal readonly string Name;

        internal MindField(string name)
        {
            Name = name;
        }
    }

    class MindField<T> : MindField
    {
        internal MindField(string name)
            : base(name)
        {

        }
    }
}
