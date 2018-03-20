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

        private readonly PointingGeneratorBase _generator;

        private readonly Body _body;

        internal BodyState2 BestState => _currentStates.OrderByDescending(s => s.Score).FirstOrDefault();

        internal StateBeam(PointingGeneratorBase generator, Body body)
        {
            _generator = generator;
            _body = body;
            _currentStates.Add(BodyState2.Empty());
        }

        internal void ShrinkTo(BodyState2 state)
        {
            _currentStates.Clear();
            _currentStates.Add(state);
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

            _currentStates = newStates;
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
            var pointings = _generator.GenerateMappings(state).ToArray();
            var pointingCovers = generateCovers(pointings);

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
                        var substitutedState = _generator.AddSubstitution(state, availableParameter, activeConcept);
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
                    if (indexes[currentIndex] >= pointingAssignments.Length)
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
                    cover[i] = pointings[indexes[i]];
                }

                yield return cover;
            }
        }

        private RankedPointing[][] getAssignments(RankedPointing[] pointings)
        {
            var assignments = new Dictionary<InputPhrase, List<RankedPointing>>();
            foreach (var pointing in pointings)
            {
                if (!assignments.TryGetValue(pointing.InputPhrase, out var inputIndex))
                    assignments[pointing.InputPhrase] = inputIndex = new List<RankedPointing>();

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
