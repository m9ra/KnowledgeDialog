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
        private readonly Phrase[] _input;

        private readonly Dictionary<PointableInstance, RankedPointing> _pointings = new Dictionary<PointableInstance, RankedPointing>();

        private readonly Dictionary<ConceptParameter, IEnumerable<PointableInstance>> _parameters = new Dictionary<ConceptParameter, IEnumerable<PointableInstance>>();

        private readonly Dictionary<Tuple<PointableInstance, PointableInstance>, PointableInstance> _indexValues = new Dictionary<Tuple<PointableInstance, PointableInstance>, PointableInstance>();

        internal Phrase LastInputPhrase => _input.LastOrDefault();

        internal IEnumerable<Phrase> InputPhrases => _input;

        internal readonly double Score;

        internal IEnumerable<ConceptParameter> AvailableParameters => _parameters.Where(p => p.Key.AllowMultipleSubtitutions || p.Value == null).Select(p => p.Key);

        internal IEnumerable<ConceptInstance> ActiveConcepts => _pointings.Values.Where(r => !_pointings.ContainsKey(r.Target)).Select(r => r.Target as ConceptInstance).Where(i => i != null).ToArray();

        internal static BodyState2 Empty()
        {
            return new BodyState2(null, 0.0, new Phrase[0], new Dictionary<PointableInstance, RankedPointing>(), new Dictionary<ConceptParameter, IEnumerable<PointableInstance>>(), new Dictionary<Tuple<PointableInstance, PointableInstance>, PointableInstance>());
        }

        private BodyState2(BodyState2 previousState, double extraScore = 0.0, Phrase[] input = null, Dictionary<PointableInstance, RankedPointing> pointings = null, Dictionary<ConceptParameter, IEnumerable<PointableInstance>> parameters = null, Dictionary<Tuple<PointableInstance, PointableInstance>, PointableInstance> indexValues = null)
        {
            _input = input ?? previousState._input;
            _pointings = pointings ?? previousState._pointings;
            _parameters = parameters ?? previousState._parameters;
            _indexValues = indexValues ?? previousState._indexValues;
            Score = (previousState == null ? 0 : previousState.Score) + extraScore;
        }

        internal RankedPointing GetRankedPointing(PointableInstance phrase)
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

        internal BodyState2 SetIndexValue(PointableInstance target, PointableInstance index, PointableInstance value)
        {
            var key = Tuple.Create(target, index);
            var newIndexValues = new Dictionary<Tuple<PointableInstance, PointableInstance>, PointableInstance>(_indexValues);

            newIndexValues[key] = value;

            return new BodyState2(this, indexValues: newIndexValues);
        }

        internal PointableInstance GetIndexValue(PointableInstance target, PointableInstance index)
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

        internal BodyState2 AddScore(double score)
        {
            return new BodyState2(this, extraScore: score);
        }

        internal BodyState2 AddPhrase(string word)
        {
            return new BodyState2(this, input: _input.Concat(new[] { Phrase.FromWord(word) }).ToArray());
        }

        internal BodyState2 DefineParameter(ConceptParameter parameter)
        {
            var newParameters = new Dictionary<ConceptParameter, IEnumerable<PointableInstance>>(_parameters);
            newParameters.Add(parameter, null);

            return new BodyState2(this, parameters: newParameters);
        }

        internal IEnumerable<PointableInstance> GetSubsitution(ConceptParameter parameter)
        {
            _parameters.TryGetValue(parameter, out var substitution);
            return substitution;
        }

        internal bool IsUsedAsParameter(ConceptInstance activeConcept)
        {
            foreach (var parameterValues in _parameters.Values)
            {
                if (parameterValues == null)
                    continue;

                if (parameterValues.Contains(activeConcept))
                    return true;
            }

            return false;
        }

        internal BodyState2 AddSubstitution(ConceptParameter parameter, PointableInstance substitution, double score)
        {
            var newParameters = new Dictionary<ConceptParameter, IEnumerable<PointableInstance>>(_parameters);
            var newSubstitutions = newParameters[parameter];
            if (newSubstitutions == null)
                newSubstitutions = Enumerable.Empty<ConceptInstance>();

            newSubstitutions = newSubstitutions.Concat(new[] { substitution }).ToArray();
            newParameters[parameter] = newSubstitutions;

            return new BodyState2(this, parameters: newParameters, extraScore: score);
        }

        internal int GetPhraseIndex(Phrase phrase)
        {
            return Array.IndexOf(_input, phrase);
        }

        internal BodyState2 Add(IEnumerable<RankedPointing> pointings)
        {
            var newPointings = new Dictionary<PointableInstance, RankedPointing>(_pointings);
            var extraScore = 0.0;
            foreach (var pointing in pointings)
            {
                extraScore += pointing.Rank;
                newPointings.Add(pointing.Source, pointing);
            }

            return new BodyState2(this, pointings: newPointings, extraScore: extraScore);
        }

        public override string ToString()
        {
            return string.Join(" ", _input.Select(i => $"[{i}]"));
        }
    }
}
