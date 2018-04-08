using PerceptiveDialogBasedAgent.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    class BodyState
    {
        /// <summary>
        /// Input that lead to this state.
        /// </summary>
        private readonly InputPhrase[] _input;

        /// <summary>
        /// Values that can be stored within the state.
        /// </summary>
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        /// <summary>
        /// Mapping of input phrases to concepts.
        /// </summary>
        private readonly Dictionary<InputPhrase, RankedConcept> _pointings = new Dictionary<InputPhrase, RankedConcept>();

        private readonly Dictionary<ConceptRequirement, IEnumerable<Concept>> _multiParameterRequirements = new Dictionary<ConceptRequirement, IEnumerable<Concept>>();

        private readonly Dictionary<ConceptRequirement, Concept> _parameterRequirements = new Dictionary<ConceptRequirement, Concept>();

        /// <summary>
        /// Score showing how probable the state is.
        /// </summary>
        public readonly double Score;

        /// <summary>
        /// Phrase added as last.
        /// </summary>
        public InputPhrase LastInputPhrase => _input.LastOrDefault();

        /// <summary>
        /// Requirements that are not fully assigned yet. (Contains also all not commited multiparameters).
        /// </summary>
        public IEnumerable<ConceptRequirement> PendingRequirements => _parameterRequirements.Where(p => p.Value == null).Select(p => p.Key).Concat(_multiParameterRequirements.Keys);

        /// <summary>
        /// Recently mentioned concepts.
        /// </summary>
        public IEnumerable<Concept> RecentMentionedConcepts => _input.Reverse().Take(10).Where(_pointings.ContainsKey).Select(i => _pointings[i].Concept).Distinct().ToArray();

        public IEnumerable<RankedConcept> RankedConcepts => _pointings.Values;

        public IEnumerable<InputPhrase> Input => _input;

        /// <summary>
        /// Copy constructor - VALUES PROVIDED AS PARAMETERS CANNOT CHANGE.
        /// </summary>
        private BodyState(double score, InputPhrase[] input, Dictionary<string, string> values, Dictionary<InputPhrase, RankedConcept> pointings, Dictionary<ConceptRequirement, Concept> parameterRequirements, Dictionary<ConceptRequirement, IEnumerable<Concept>> multiParameterRequirements)
        {
            _input = input;
            _values = values;
            _pointings = pointings;
            _parameterRequirements = parameterRequirements;
            _multiParameterRequirements = multiParameterRequirements;
            Score = score;
        }

        internal BodyState()
        {
            _input = new InputPhrase[0];
            Score = 0;
        }


        internal RankedConcept GetConcept(InputPhrase phrase)
        {
            _pointings.TryGetValue(phrase, out var result);
            return result;
        }

        internal string GetValue(string key)
        {
            _values.TryGetValue(key, out var value);
            return value;
        }

        internal Concept GetParameter(ConceptRequirement requirement)
        {
            _parameterRequirements.TryGetValue(requirement, out var result);
            return result;
        }

        internal IEnumerable<Concept> GetMultiParameter(ConceptRequirement requirement)
        {
            return _multiParameterRequirements[requirement];
        }

        internal BodyState AssignParameter(ConceptRequirement requirement, Concept concept, double score)
        {
            if (_parameterRequirements.ContainsKey(requirement) && _parameterRequirements[requirement] != concept)
            {
                var newParameterRequirements = new Dictionary<ConceptRequirement, Concept>(_parameterRequirements);
                newParameterRequirements[requirement] = concept;
                return new BodyState(Score, _input, _values, _pointings, newParameterRequirements, _multiParameterRequirements);
            }

            if (_multiParameterRequirements.ContainsKey(requirement) && !_multiParameterRequirements[requirement].Contains(concept))
            {
                var newMultiParameterRequirements = new Dictionary<ConceptRequirement, IEnumerable<Concept>>(_multiParameterRequirements);
                var parameters = new HashSet<Concept>(_multiParameterRequirements[requirement]);
                parameters.Add(concept);
                newMultiParameterRequirements[requirement] = parameters;
                return new BodyState(Score + score, _input, _values, _pointings, _parameterRequirements, newMultiParameterRequirements);
            }
            return this;
        }

        internal BodyState SetValue(string variable, string value)
        {
            var newValues = new Dictionary<string, string>(_values);
            newValues[variable] = value;

            return new BodyState(Score, _input, newValues, _pointings, _parameterRequirements, _multiParameterRequirements);
        }

        internal BodyState AddRequirement(ConceptRequirement requirement)
        {
            var newRequirements = new Dictionary<ConceptRequirement, Concept>(_parameterRequirements);
            newRequirements.Add(requirement, null);

            return new BodyState(Score, _input, _values, _pointings, newRequirements, _multiParameterRequirements);
        }

        internal BodyState AddMultiRequirement(ConceptRequirement requirement)
        {
            var newRequirements = new Dictionary<ConceptRequirement, IEnumerable<Concept>>(_multiParameterRequirements);
            newRequirements.Add(requirement, null);

            return new BodyState(Score, _input, _values, _pointings, _parameterRequirements, newRequirements);
        }

        internal BodyState ExpandLastPhrase(string word)
        {
            var newInput = new List<InputPhrase>();
            newInput.AddRange(_input);

            newInput[newInput.Count - 1] = LastInputPhrase.ExpandBy(word);
            return new BodyState(Score, newInput.ToArray(), _values, _pointings, _parameterRequirements, _multiParameterRequirements);
        }

        internal BodyState AddNewPhrase(string word)
        {
            var newInput = new List<InputPhrase>();
            newInput.AddRange(_input);
            newInput.Add(InputPhrase.FromWord(word));

            return new BodyState(Score, newInput.ToArray(), _values, _pointings, _parameterRequirements, _multiParameterRequirements);
        }

        internal BodyState SetPointer(InputPhrase phrase, RankedConcept concept)
        {
            var newPointings = new Dictionary<InputPhrase, RankedConcept>(_pointings);
            newPointings.Add(phrase, concept);

            return new BodyState(Score + concept.Rank, _input, _values, newPointings, _parameterRequirements, _multiParameterRequirements);
        }
    }
}
