using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V1.SemanticRepresentation
{
    class SemanticPattern
    {
        /// <summary>
        /// Represents a pattern.
        /// </summary>
        internal string Representation => string.Join(" ", _patternParts);

        /// <summary>
        /// How many parts the pattern contains.
        /// </summary>
        internal int PartCount => _patternParts.Length;

        internal IEnumerable<string> Parts => _patternParts;

        /// <summary>
        /// Parts of the pattern.
        /// </summary>
        private readonly string[] _patternParts;

        private SemanticPattern(string[] patternParts)
        {
            _patternParts = patternParts.ToArray();
        }

        internal static SemanticPattern Raw(string expression)
        {
            return new SemanticPattern(new[] { expression });
        }

        internal static SemanticPattern Parse(string[] patternParts)
        {
            var variables = new HashSet<string>();
            foreach (var part in patternParts)
            {
                if (part.StartsWith("$"))
                    variables.Add(part);
            }

            //TODO pass the variables info
            return new SemanticPattern(patternParts);
        }

        internal bool IsVariable(int currentPatternPart)
        {
            var patternPart = GetCurrentPart(currentPatternPart);
            return patternPart.StartsWith("$");
        }

        internal string GetCurrentPart(int currentPatternPart)
        {
            return _patternParts[currentPatternPart];
        }
    }
}
