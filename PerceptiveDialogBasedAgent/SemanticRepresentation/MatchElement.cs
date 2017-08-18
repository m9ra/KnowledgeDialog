using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.SemanticRepresentation
{
    class MatchElement
    {
        /// <summary>
        /// Parent containing this element.
        /// </summary>
        internal MatchElement Parent { get; private set; }

        /// <summary>
        /// Pattern corresponding to this element.
        /// </summary>
        internal readonly SemanticPattern Pattern;

        /// <summary>
        /// Token that caused matching of this element.
        /// </summary>
        internal readonly string Token;

        /// <summary>
        /// Substituions registered for this element.
        /// </summary>
        private readonly Dictionary<string, MatchElement> _substitutions = new Dictionary<string, MatchElement>();

        internal MatchElement(string token)
        {
            Token = token;
            Pattern = SemanticPattern.Raw(Token);
        }

        internal MatchElement GetSubstitution(string variable)
        {
            return _substitutions[variable];
        }

        internal MatchElement(SemanticPattern pattern, Dictionary<string, MatchElement> substitutions)
        {
            Pattern = pattern;
            Token = Pattern.Representation;

            foreach (var substitution in substitutions)
            {
                var child = substitution.Value;
                var variable = substitution.Key;

                registerChild(child);
                _substitutions.Add(variable, child);
                Token = Token.Replace(variable, child.Token);
            }
        }

        private void registerChild(MatchElement child)
        {
            if (child.Parent != null)
                throw new NotSupportedException("Cannot register child with parent");

            child.Parent = this;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = "";
            foreach (var word in Pattern.Parts)
            {
                if (result.Length > 0)
                    result += " ";

                result += word;
                if (_substitutions.ContainsKey(word))
                    result += "(" + _substitutions[word] + ")";
            }

            return result;
        }
    }
}
