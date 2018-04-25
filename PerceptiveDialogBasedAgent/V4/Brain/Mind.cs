using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    class Mind
    {
        private List<MindState> _currentBeam = new List<MindState>();

        public MindState BestState => _currentBeam.OrderByDescending(s => s.Score).FirstOrDefault();

        internal void Accept(IEnumerable<PointableInstance> activeInstances, PropertyContainer container)
        {
            var newBeam = new List<MindState>();
            foreach (var state in _currentBeam)
            {
                foreach (var newState in expand(state, activeInstances, container))
                {
                    newBeam.Add(newState);
                }
            }

            _currentBeam = newBeam;
        }

        internal void SetBeam(MindState mindState)
        {
            _currentBeam = new List<MindState>();
            _currentBeam.Add(mindState);
        }

        internal void NewTurnEvent()
        {
            var evt = new ConceptInstance(Concept2.NewTurn);

            var newStates = new List<MindState>();
            foreach (var state in _currentBeam)
            {
                newStates.Add(state.AddEvent(evt));
            }
            _currentBeam.Clear();
            _currentBeam.AddRange(newStates);
        }

        internal IEnumerable<MindState> expand(MindState state, IEnumerable<PointableInstance> instances, PropertyContainer container)
        {
            var currentStates = new List<MindState>();
            currentStates.Add(state);
            var newStates = new List<MindState>();
            for (var i = 0; i < 5; ++i) //limit interation count
            {
                //try to set some substitutions
                foreach (var currentState in currentStates)
                {
                    newStates.Add(currentState); //no substitution change
                    foreach (var instance in instances)
                    {
                        //import property values between states
                        var importedState = currentState.Import(instance, container);
                        foreach (var point in importedState.GetSubstitutionPoints())
                        {
                            var substitutedState = point.Substitute(instance);
                            if (substitutedState == null)
                                continue;

                            newStates.Add(substitutedState);
                        }
                    }
                }

                //shrink new states
                var oldCurrentStates = currentStates;
                currentStates = newStates;
                newStates = oldCurrentStates;
                newStates.Clear();
            }

            return currentStates;
        }
    }
}
