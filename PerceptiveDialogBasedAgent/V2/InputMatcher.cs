using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class InputMatcher
    {
        internal IEnumerable<SemanticItem> Match(SemanticItem pattern, SemanticItem input)
        {
            pattern = pattern.WithConstraints(pattern.Constraints.AddConditions(input.Constraints.Conditions));
            var matchFactory = getMatchFactory(pattern);

            if (matchFactory == null)
                return new SemanticItem[0];

            var result = matchFactory.Generate(input).ToArray();
            return result;
        }

        private MatchFactory getMatchFactory(SemanticItem pattern)
        {
            return new MatchFactory(pattern);
        }
    }
}
