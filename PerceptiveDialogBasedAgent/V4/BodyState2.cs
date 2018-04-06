using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PerceptiveDialogBasedAgent.V3;

namespace PerceptiveDialogBasedAgent.V4
{
    class BodyState2
    {
        private readonly InputPhrase[] _input;

        private readonly Dictionary<InputPhrase, RankedPointing> _pointings = new Dictionary<InputPhrase, RankedPointing>();

        private readonly Dictionary<ConceptParameter, IEnumerable<ConceptInstance>> _parameters = new Dictionary<ConceptParameter, IEnumerable<ConceptInstance>>();

        private readonly Dictionary<Tuple<ConceptInstance, ConceptInstance>, ConceptInstance> _indexValues = new Dictionary<Tuple<ConceptInstance, ConceptInstance>, ConceptInstance>();

        internal InputPhrase LastInputPhrase => _input.LastOrDefault();

        internal IEnumerable<InputPhrase> InputPhrases => _input;

        internal readonly double Score;

        internal IEnumerable<ConceptParameter> AvailableParameters => _parameters.Where(p => p.Key.AllowMultipleSubtitutions || p.Value == null).Select(p => p.Key);

        internal IEnumerable<ConceptInstance> ActiveConcepts => _pointings.Values.Select(r => r.Target as ConceptInstance).Where(i => i != null).ToArray();

        internal static BodyState2 Empty()
        {
            return new BodyState2(null, 0.0, new InputPhrase[0], new Dictionary<InputPhrase, RankedPointing>(), new Dictionary<ConceptParameter, IEnumerable<ConceptInstance>>(), new Dictionary<Tuple<ConceptInstance, ConceptInstance>, ConceptInstance>());
        }

        private BodyState2(BodyState2 previousState, double extraScore = 0.0, InputPhrase[] input = null, Dictionary<InputPhrase, RankedPointing> pointings = null, Dictionary<ConceptParameter, IEnumerable<ConceptInstance>> parameters = null, Dictionary<Tuple<ConceptInstance, ConceptInstance>, ConceptInstance> indexValues = null)
        {
            _input = input ?? previousState._input;
            _pointings = pointings ?? previousState._pointings;
            _parameters = parameters ?? previousState._parameters;
            _indexValues = indexValues ?? previousState._indexValues;
            Score = (previousState == null ? 0 : previousState.Score) + extraScore;
        }

        internal RankedPointing GetRankedPointing(InputPhrase phrase)
        {
            _pointings.TryGetValue(phrase, out var rankedPointing);

            return rankedPointing;
        }

        internal bool ContainsSubstitutionFor(ConceptParameter parameter)
        {
            return _parameters.TryGetValue(parameter, out var substitutions) && substitutions != null;
        }

        internal bool IsDefined(ConceptParameter parameter)
        {
            return _parameters.TryGetValue(parameter, out var substitutions);
        }

        internal BodyState2 SetIndexValue(ConceptInstance target, ConceptInstance index, ConceptInstance value)
        {
            var key = Tuple.Create(target, index);
            var newIndexValues = new Dictionary<Tuple<ConceptInstance, ConceptInstance>, ConceptInstance>(_indexValues);

            newIndexValues[key] = value;

            return new BodyState2(this, indexValues: newIndexValues);
        }

        internal ConceptInstance GetIndexValue(ConceptInstance target, ConceptInstance index)
        {
            _indexValues.TryGetValue(Tuple.Create(target, index), out var result);
            return result;
        }

        internal BodyState2 ExpandLastPhrase(string word)
        {
            if (_input.Length == 0)
                return null;

            if (_pointings.ContainsKey(LastInputPhrase))
                //phrase that was used for pointing cannot be expanded
                return null;

            var newPhrase = _input.Last().ExpandBy(word);
            var newInput = _input.ToArray();
            newInput[newInput.Length - 1] = newPhrase;
            return new BodyState2(this, input: newInput);
        }

        internal BodyState2 AddPhrase(string word)
        {
            return new BodyState2(this, input: _input.Concat(new[] { InputPhrase.FromWord(word) }).ToArray());
        }

        internal BodyState2 DefineParameter(ConceptParameter parameter)
        {
            var newParameters = new Dictionary<ConceptParameter, IEnumerable<ConceptInstance>>(_parameters);
            newParameters.Add(parameter, null);

            return new BodyState2(this, parameters: newParameters);
        }

        internal IEnumerable<ConceptInstance> GetSubsitution(ConceptParameter parameter)
        {
            _parameters.TryGetValue(parameter, out var substitution);
            return substitution;
        }

        internal BodyState2 AddSubstitution(ConceptParameter parameter, ConceptInstance substitution, double score)
        {
            var newParameters = new Dictionary<ConceptParameter, IEnumerable<ConceptInstance>>(_parameters);
            var newSubstitutions = newParameters[parameter];
            if (newSubstitutions == null)
                newSubstitutions = Enumerable.Empty<ConceptInstance>();

            newSubstitutions = newSubstitutions.Concat(new[] { substitution }).ToArray();
            newParameters[parameter] = newSubstitutions;

            return new BodyState2(this, parameters: newParameters, extraScore: score);
        }

        internal BodyState2 Add(IEnumerable<RankedPointing> pointings)
        {
            var newPointings = new Dictionary<InputPhrase, RankedPointing>(_pointings);
            var extraScore = 0.0;
            foreach (var pointing in pointings)
            {
                extraScore += pointing.Rank;
                newPointings.Add(pointing.InputPhrase, pointing);
            }

            return new BodyState2(this, pointings: newPointings, extraScore: extraScore);
        }

        public override string ToString()
        {
            return string.Join(" ", _input.Select(i => $"[{i}]"));
        }
    }
}
