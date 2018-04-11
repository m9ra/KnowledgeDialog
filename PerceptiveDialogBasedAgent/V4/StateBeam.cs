using PerceptiveDialogBasedAgent.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class StateBeam
    {
        private List<BodyState2> _currentStates = new List<BodyState2>();

        private readonly Body _body;

        internal IEnumerable<BodyState2> BodyStates => _currentStates;

        internal BodyState2 BestState => _currentStates.OrderByDescending(s => s.Score).FirstOrDefault();

        internal StateBeam(Body body)
        {
            _body = body;
            _currentStates.Add(BodyState2.Empty());
        }

        internal void SetBeam(BodyState2 state)
        {
            SetBeam(new[] { state });
        }

        internal void SetBeam(IEnumerable<BodyState2> states)
        {
            var statesCopy = states.ToArray();
            _currentStates.Clear();
            _currentStates.AddRange(statesCopy);
        }

        internal void ExpandBy(string word)
        {
            var newStates = new List<BodyState2>();
            foreach (var state in _currentStates)
            {
                foreach (var expandedState in expandState(state, word))
                {
                    foreach (var stateWithPointings in findPointings(expandedState))
                    {
                        foreach (var stateWithParameters in substituteParameters(stateWithPointings))
                        {
                            newStates.Add(stateWithParameters);
                        }
                    }
                }
            }

            newStates = newStates.OrderByDescending(s => s.Score).ToList();

            var limit = 300;
            if (newStates.Count > limit)
            {
                _currentStates = newStates.Take(limit).ToList();
            }
            else {
                _currentStates = newStates;
            }
        }

        private IEnumerable<BodyState2> expandState(BodyState2 state, string word)
        {
            var expanded = state.ExpandLastPhrase(word);
            if (expanded != null)
                yield return expanded;

            yield return state.AddPhrase(word);
        }

        private IEnumerable<BodyState2> findPointings(BodyState2 state)
        {
            var pointings = _body.Model.GenerateMappings(state).ToArray();
            var forwardedPointings = new List<RankedPointing>();
            for (var i = 0; i < pointings.Length; ++i)
            {
                //TODO this should be handled in a better way
                foreach (var forwardedPointing in getForwarding(pointings[i], state))
                {
                    forwardedPointings.Add(forwardedPointing);
                }
            }

            var pointingCovers = generateCovers(forwardedPointings.ToArray()).ToArray();

            yield return state;
            foreach (var subset in pointingCovers)
            {
                var stateWithPointings = state.Add(subset);
                var processedState = stateWithPointings;
                foreach (var pointing in subset)
                {
                    processedState = evaluateParameterChange(pointing.Target, processedState);
                }

                yield return processedState;
            }
        }

        private IEnumerable<RankedPointing> getForwarding(RankedPointing pointing, BodyState2 state)
        {
            yield return pointing;

            var instance = pointing.Target as ConceptInstance;
            if (instance == null || instance.Concept.IsNative)
                yield break;

            foreach (var newPointing in _body.Model.GetForwardings(instance, state))
            {
                yield return newPointing;
            }
        }

        private IEnumerable<BodyState2> substituteParameters(BodyState2 state)
        {
            yield return state; //no substitution
            foreach (var availableParameter in state.AvailableParameters)
            {
                foreach (var activeConcept in state.ActiveConcepts)
                {
                    if (activeConcept == availableParameter.Owner)
                        //Parameteric self references are not allowed
                        continue;

                    if (availableParameter.IsAllowedForSubstitution(activeConcept, state))
                    {
                        //todo think about setting multiple substitutions for a single parameter
                        var substitutedState = _body.Model.AddSubstitution(state, availableParameter, activeConcept);
                        yield return evaluateParameterChange(availableParameter.Owner, substitutedState);
                    }
                }
            }
        }

        private BodyState2 evaluateParameterChange(PointableBase pointableInstance, BodyState2 state)
        {
            var instance = pointableInstance as ConceptInstance;
            if (instance == null || instance.Concept.Action == null)
                return state;

            var context = new BodyContext2(instance, _body, state);
            instance.Concept.Action(context);

            return context.CurrentState;
        }

        private IEnumerable<IEnumerable<RankedPointing>> generateCovers(RankedPointing[] pointings)
        {
            if (pointings.Length == 0)
                yield break;

            var pointingAssignments = getAssignments(pointings);
            var indexes = new int[pointingAssignments.Length];
            indexes[0] -= 1;

            while (true)
            {
                var currentIndex = 0;
                var carryBit = 1;
                while (carryBit > 0)
                {
                    if (currentIndex >= indexes.Length)
                        yield break;

                    indexes[currentIndex] += 1;
                    if (indexes[currentIndex] >= pointingAssignments[currentIndex].Length)
                    {
                        carryBit = 1;
                        indexes[currentIndex] = 0;
                    }
                    else
                    {
                        carryBit = 0;
                    }

                    ++currentIndex;
                }

                var cover = new RankedPointing[indexes.Length];
                for (var i = 0; i < indexes.Length; ++i)
                {
                    cover[i] = pointingAssignments[i][indexes[i]];
                }

                yield return cover;
            }
        }

        private RankedPointing[][] getAssignments(RankedPointing[] pointings)
        {
            var assignments = new Dictionary<PointableBase, List<RankedPointing>>();
            foreach (var pointing in pointings)
            {
                if (!assignments.TryGetValue(pointing.Source, out var inputIndex))
                    assignments[pointing.Source] = inputIndex = new List<RankedPointing>();

                inputIndex.Add(pointing);
            }

            return assignments.Select(s => s.Value.ToArray()).ToArray();
        }

        public static IEnumerable<IEnumerable<T>> SubSetsOf<T>(IEnumerable<T> source)
        {
            if (!source.Any())
                return Enumerable.Repeat(Enumerable.Empty<T>(), 1);

            var element = source.Take(1);

            var haveNots = SubSetsOf(source.Skip(1));
            var haves = haveNots.Select(set => element.Concat(set));

            return haves.Concat(haveNots);
        }
    }
}
