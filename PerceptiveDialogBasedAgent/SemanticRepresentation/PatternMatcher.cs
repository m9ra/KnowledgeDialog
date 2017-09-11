using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.SemanticRepresentation
{
    class PatternMatcher
    {
        /// <summary>
        /// List of recognized patterns.
        /// </summary>
        private readonly List<SemanticPattern> _patterns = new List<SemanticPattern>();

        internal void AddPattern(SemanticPattern pattern)
        {
            _patterns.Add(pattern);
        }

        internal Match BestMatch(string expression)
        {
            var matches = Match(expression);
            return matches.FirstOrDefault();
        }

        internal IEnumerable<Match> Match(string expression)
        {
            var tokens = tokenize(expression);

            var progresses = Match(tokens);

            var result = new List<Match>();

            var rootElements = new List<MatchElement>();
            foreach (var progress in progresses)
            {
                var element = progress.CreateTopElement();
                rootElements.Add(element);

                //TODO  more root elements will be also possible
                result.Add(new Match(element));
            }

            if (result.Count == 0)
                result.Add(new Match(new MatchElement(expression)));

            return result;
        }

        internal IEnumerable<PatternMatchProgress> Match(MatchElement[] elements)
        {
            var workList = _patterns.Select(pattern => new PatternMatchProgress(pattern, elements)).ToArray();

            for (var i = 0; i < elements.Length; ++i)
            {
                if (workList.Length == 0)
                    //no match is available
                    break;

                var currentToken = elements[i];

                var newWorkList = new List<PatternMatchProgress>();
                foreach (var currentProgress in workList)
                {
                    var newProgresses = currentProgress.MakeStep(currentToken);
                    newWorkList.AddRange(newProgresses);
                }

                workList = newWorkList.ToArray();
            }

            var satisfiedProgresses = workList.Where(p => p.CanBeFinished).ToArray();
            foreach (var progress in satisfiedProgresses)
            {
                progress.SubstituteBuffers(this);
            }

            return satisfiedProgresses;
        }

        private MatchElement[] tokenize(string expression)
        {
            return expression.Split(' ').Select(t => new MatchElement(t)).ToArray();
        }
    }


    class PatternMatchProgress
    {
        internal readonly SemanticPattern Pattern;

        internal bool CanBeFinished => _currentPatternPart == Pattern.PartCount && _varBuffers.All(b => b == null || b.Length > 0);

        private readonly int _currentPatternPart = 0;

        private readonly MatchElement[][] _varBuffers;

        private PatternMatchProgress[][] _varSubstitutions;

        internal PatternMatchProgress(SemanticPattern pattern, MatchElement[] tokens)
        {
            Pattern = pattern;
            _varBuffers = new MatchElement[Pattern.PartCount][];
            _varSubstitutions = new PatternMatchProgress[_varBuffers.Length][];
        }

        private PatternMatchProgress(SemanticPattern pattern, int currentPatternPart, MatchElement[][] varBuffers, PatternMatchProgress[][] varSubstitutions)
        {
            Pattern = pattern;
            _currentPatternPart = currentPatternPart;
            _varSubstitutions = varSubstitutions;
            _varBuffers = varBuffers; // we don't need copying because ctor is private
        }

        internal MatchElement CreateTopElement()
        {
            var instantiatedSubstitutions = new Dictionary<string, MatchElement>();
            for (var i = 0; i < _varSubstitutions.Length; ++i)
            {
                var substitution = _varSubstitutions[i];
                if (substitution == null)
                    continue;

                var variableName = Pattern.GetCurrentPart(i);
                if (substitution.Length == 0)
                {
                    var rawSubstitution = string.Join(" ", _varBuffers[i].Select(v => v.Pattern.Representation));
                    instantiatedSubstitutions.Add(variableName, new MatchElement(rawSubstitution));
                }
                else
                {
                    instantiatedSubstitutions.Add(variableName, substitution[0].CreateTopElement());
                }
            }

            return new MatchElement(Pattern, instantiatedSubstitutions);
        }

        internal IEnumerable<PatternMatchProgress> MakeStep(MatchElement currentElement)
        {
            if (_currentPatternPart >= Pattern.PartCount)
                yield break;

            var isVariable = Pattern.IsVariable(_currentPatternPart);
            var currentPatternPart = Pattern.GetCurrentPart(_currentPatternPart);
            var isPartHardMatched = !isVariable && currentElement.Pattern.Representation.Equals(currentPatternPart);

            if (isPartHardMatched)
            {
                //only way hard match can appear is via non-variable
                yield return NextStep();
            }

            if (isVariable)
            {
                var newVarBuffers = extendBuffers(currentElement);

                //first possibility, variable ends after this word.
                yield return new PatternMatchProgress(Pattern, _currentPatternPart + 1, newVarBuffers, _varSubstitutions);
                //second, variable accepts the word keep accepting other word(s)
                yield return new PatternMatchProgress(Pattern, _currentPatternPart, newVarBuffers, _varSubstitutions);
            }
        }

        private MatchElement[][] extendBuffers(MatchElement currentElement)
        {
            var extendedBuffers = new MatchElement[_varBuffers.Length][];
            Array.Copy(_varBuffers, extendedBuffers, extendedBuffers.Length);


            var previousBuffer = extendedBuffers[_currentPatternPart];
            if (previousBuffer == null)
                extendedBuffers[_currentPatternPart] = new[] { currentElement };
            else
                extendedBuffers[_currentPatternPart] = previousBuffer.Concat(new[] { currentElement }).ToArray();

            return extendedBuffers;
        }

        internal PatternMatchProgress NextStep()
        {
            return new PatternMatchProgress(Pattern, _currentPatternPart + 1, _varBuffers, _varSubstitutions);
        }

        internal void SubstituteBuffers(PatternMatcher matcher)
        {
            for (var i = 0; i < _varBuffers.Length; ++i)
            {
                var buffer = _varBuffers[i];
                if (buffer == null)
                    continue;

                var substitutions = matcher.Match(buffer);
                _varSubstitutions[i] = substitutions.ToArray();
            }
        }
    }
}
