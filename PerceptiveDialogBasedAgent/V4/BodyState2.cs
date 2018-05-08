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

        internal readonly PropertyContainer PropertyContainer;

        internal Phrase LastInputPhrase => _input.LastOrDefault();

        internal IEnumerable<Phrase> InputPhrases => _input;

        internal readonly double Score;

        internal IEnumerable<Tuple<PointableInstance, Concept2>> AllParameters => ActiveConcepts.SelectMany(c => c.Concept.Properties.Where(p => IsParameter(p)).Select(p => Tuple.Create(c as PointableInstance, p))).ToArray();

        internal IEnumerable<Tuple<PointableInstance, Concept2>> AvailableParameters => AllParameters.Where(p => !PropertyContainer.ContainsKey(p)).ToArray();

        internal IEnumerable<ConceptInstance> ActiveConcepts => _pointings.Values.Where(r => !_pointings.ContainsKey(r.Target)).Select(r => r.Target as ConceptInstance).Where(i => i != null).ToArray();

        internal static BodyState2 Empty()
        {
            return new BodyState2(null, 0.0, new Phrase[0], new Dictionary<PointableInstance, RankedPointing>(), new PropertyContainer());
        }

        private BodyState2(BodyState2 previousState, double extraScore = 0.0, Phrase[] input = null, Dictionary<PointableInstance, RankedPointing> pointings = null, PropertyContainer container = null)
        {
            _input = input ?? previousState._input;
            _pointings = pointings ?? previousState._pointings;
            PropertyContainer = container ?? previousState.PropertyContainer;
            Score = (previousState == null ? 0 : previousState.Score) + extraScore;
        }

        internal RankedPointing GetRankedPointing(PointableInstance phrase)
        {
            _pointings.TryGetValue(phrase, out var rankedPointing);

            return rankedPointing;
        }

        internal bool ContainsSubstitutionFor(PointableInstance container, Concept2 parameter)
        {
            return PropertyContainer.ContainsSubstitutionFor(container, parameter);
        }

        internal BodyState2 SetPropertyValue(PointableInstance target, Concept2 property, PointableInstance value)
        {
            var newContainer = PropertyContainer.SetPropertyValue(target, property, value);

            return new BodyState2(this, container: newContainer);
        }

        internal PointableInstance GetPropertyValue(PointableInstance target, Concept2 property)
        {
            return PropertyContainer.GetPropertyValue(target, property);
        }

        internal bool IsParameter(Concept2 property)
        {
            return PropertyContainer.IsParameter(property);
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

        internal PointableInstance GetParameterValue(PointableInstance container, Concept2 parameter)
        {
            return GetPropertyValue(container, parameter);
        }

        internal BodyState2 SetSubstitution(PointableInstance owner, Concept2 parameter, PointableInstance value, double score)
        {
            var result = SetPropertyValue(owner, parameter, value);
            return result.AddScore(score);
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

        internal BodyState2 Import(PointableInstance instance, PropertyContainer container)
        {
            var newContainer = PropertyContainer.Import(instance, container);
            var newPointings = new Dictionary<PointableInstance, RankedPointing>(_pointings);

            newPointings[instance.ActivationPhrase] = new RankedPointing(instance.ActivationPhrase, instance, 1.0);
            return new BodyState2(this, container: newContainer, pointings: newPointings);
        }
    }
}
