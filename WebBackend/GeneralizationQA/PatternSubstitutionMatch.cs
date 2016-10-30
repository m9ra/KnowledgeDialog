using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.GeneralizationQA
{
    class PatternSubstitutionMatch
    {
        /// <summary>
        /// How well the substitution matches to the pattern.
        /// </summary>
        public readonly double Rank;

        /// <summary>
        /// Paths that were found for substitution.
        /// </summary>
        public readonly IEnumerable<PathSubstitution> SubstitutionPaths;

        public PatternSubstitutionMatch(IEnumerable<PathSubstitution> substitutionPaths)
        {
            SubstitutionPaths = substitutionPaths.ToArray();
            var matchProbability = 1.0;
            foreach (var path in SubstitutionPaths)
            {
                matchProbability = matchProbability * path.Rank;
            }
        }
    }
}
