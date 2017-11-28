using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class MatchFactory
    {
        private readonly SemanticItem _pattern;

        private readonly MatchPart[] _parts;

        internal MatchFactory(SemanticItem patternItem)
        {
            _pattern = patternItem;

            _parts = createParts(patternItem.Constraints.Input);
        }

        internal IEnumerable<SemanticItem> Generate(SemanticItem input)
        {
            var inputText = input.InstantiateInputWithEntityVariables();
            

            var inputTokens = tokenize(inputText);
            initializeParts(inputText);

            while (true)
            {
                var item = generateCurrentState(inputTokens, _pattern.Constraints);
                if (item != null)
                    yield return item;

                var i = 0;
                while (!_parts[i].ShiftNext())
                {
                    i += 1;
                    if (i >= _parts.Length)
                        yield break;

                    _parts[i - 1].Reset();
                }
            }
        }

        private SemanticItem generateCurrentState(string[] inputTokens, Constraints inputConstraints)
        {
            var isValid = _parts.All(p => p.TryToValidate());
            var totalLength = _parts.Select(p => p.CurrentLength).Sum();
            if (!isValid || totalLength != inputTokens.Length)
                return null;

            var constraints = inputConstraints;
            for (var i = 0; i < _parts.Length; ++i)
            {
                constraints = _parts[i].Substitute(constraints);
            }

            return _pattern.WithConstraints(constraints);
        }

        private void initializeParts(string input)
        {
            var inputTokens = tokenize(input);
            for (var i = 0; i < _parts.Length; ++i)
            {
                var previous = i > 0 ? _parts[i - 1] : null;
                var next = i + 1 < _parts.Length ? _parts[i + 1] : null;
                _parts[i].Initialize(inputTokens, previous, next);
            }

            foreach (var part in _parts)
            {
                part.Reset();
            }
        }

        private MatchPart[] createParts(string pattern)
        {
            var patternTokens = tokenize(pattern);

            var result = new List<MatchPart>();
            MatchPart currentPart = null;
            for (var i = 0; i < patternTokens.Length; ++i)
            {
                var token = patternTokens[i];
                if (currentPart != null && !currentPart.CanAccept(token))
                {
                    result.Add(currentPart);
                    currentPart = null;
                }

                if (currentPart == null)
                    currentPart = new MatchPart();

                currentPart.Add(token);
            }

            if (currentPart != null)
                result.Add(currentPart);

            return result.ToArray();
        }

        private string[] tokenize(string pattern)
        {
            return pattern.Split(' ');
        }
    }

    class MatchPart
    {
        private readonly List<string> _tokens = new List<string>();

        private string[] _input;

        private int _currentMatchStart = 0;

        private int _currentMatchEnd = 0;

        private List<int> _availableFixedPositions = new List<int>();

        private int _currentFixedPosition = 0;

        private int[] _variableLenghts;

        MatchPart _previous, _following;

        internal int CurrentLength => _currentMatchEnd - _currentMatchStart;

        internal bool TryToValidate()
        {
            if (!containsVariables())
                return _availableFixedPositions.Count > 0;

            setBoundaries();

            if (_currentMatchEnd > _input.Length)
                return false;

            return true;
        }

        internal bool ShiftNext()
        {
            if (containsVariables())
            {
                return shiftNextVariable();
            }
            else
            {
                return shiftNextFixed();
            }
        }

        internal bool shiftNextVariable()
        {
            setBoundaries(); //this is correct because surrounding fixed parts are always in updated state
            var maxLength = _currentMatchEnd - _currentMatchStart - 1;

            for (var i = 0; i < _variableLenghts.Length; ++i)
            {
                _variableLenghts[i] += 1;
                if (_variableLenghts.Sum() < maxLength)
                {
                    return true;
                }

                _variableLenghts[i] = 1;
            }

            return false;
        }

        internal bool shiftNextFixed()
        {
            if (_currentFixedPosition + 1 >= _availableFixedPositions.Count)
                return false;

            _currentFixedPosition += 1;
            setBoundaries();
            return true;
        }

        internal void Add(string token)
        {
            _tokens.Add(token);
        }

        internal bool CanAccept(string token)
        {
            if (_tokens.Count == 0)
                return true;

            return isVariable(token) == isVariable(_tokens[0]);
        }

        internal Constraints Substitute(Constraints constraints)
        {
            if (!containsVariables())
                return constraints;

            var currentConstraints = constraints;
            var currentOffset = _currentMatchStart;
            for (var i = 0; i < _tokens.Count - 1; ++i)
            {
                var variable = _tokens[i];
                var length = _variableLenghts[i];
                var value = string.Join(" ", _input.Skip(currentOffset).Take(length));
                var entityValue = SemanticItem.Entity(value);

                currentOffset += length;

                currentConstraints = currentConstraints.AddValue(variable, entityValue);
            }

            var lastValue = string.Join(" ", _input.Skip(currentOffset).Take(_currentMatchEnd - currentOffset));
            var entityLastValue = SemanticItem.Entity(lastValue);
            currentConstraints = currentConstraints.AddValue(_tokens.Last(), entityLastValue);
            return currentConstraints;
        }

        private void setBoundaries()
        {
            if (containsVariables())
            {
                //we can use following/previous because all variable parts are surrounded by fixed parts
                _currentMatchStart = _previous == null ? 0 : _previous._currentMatchEnd;
                _currentMatchEnd = _following == null ? _input.Length : _following._currentMatchStart;
            }
            else
            {
                if (_availableFixedPositions.Count == 0)
                    return;

                _currentMatchStart = _availableFixedPositions[_currentFixedPosition];
                _currentMatchEnd = _currentMatchStart + _tokens.Count;
            }
        }

        private bool isVariable(string token)
        {
            return token.StartsWith("$");
        }

        private bool containsVariables()
        {
            return isVariable(_tokens[0]);
        }

        internal void Initialize(string[] inputTokens, MatchPart previous, MatchPart following)
        {
            _previous = previous;
            _following = following;

            _input = inputTokens;
            if (containsVariables())
            {
                _variableLenghts = new int[_tokens.Count - 1];
                return;
            }

            for (var i = 0; i < _input.Length; ++i)
            {
                var isMatch = true;
                for (var j = 0; j < _tokens.Count; ++j)
                {
                    if (i + j >= _input.Length)
                        return;

                    var currentToken = _input[i + j];
                    if (!currentToken.Equals(_tokens[j]))
                    {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch)
                    _availableFixedPositions.Add(i);
            }
        }

        internal void Reset()
        {
            if (containsVariables())
            {
                for (var i = 0; i < _variableLenghts.Length; ++i)
                    _variableLenghts[i] = 1;
            }
            else
            {
                _currentFixedPosition = 0;
            }

            setBoundaries();
        }
    }
}
