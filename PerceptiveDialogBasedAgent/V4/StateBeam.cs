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

        internal StateBeam(PointingGeneratorBase generator)
        {
            _generator = generator;
            _currentStates.Add(BodyState2.Empty());
        }

        internal void ExpandBy(string word)
        {
            var newStates = new List<BodyState2>();
            foreach (var state in _currentStates)
            {
                foreach (var expandedState in expandState(state, word))
                {
                    foreach (var processedState in processState(expandedState))
                    {
                        newStates.Add(processedState);
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

        private IEnumerable<BodyState2> processState(BodyState2 state)
        {
            var pointings = _generator.GenerateMappings(state).ToArray();
            var pointingCovers = generateCovers(pointings);

            yield return state;
            foreach (var subset in pointingCovers)
            {
                var processedState = state.Add(subset);
                yield return processedState;
            }
        }

        private IEnumerable<IEnumerable<RankedPointing>> generateCovers(RankedPointing[] pointings)
        {
            var pointingAssignments = getAssignments(pointings);
            var indexes = new int[pointingAssignments.Length];

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
