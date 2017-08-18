using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.SemanticRepresentation
{
    class Match
    {
        /// <summary>
        /// Root elements.
        /// </summary>
        internal readonly MatchElement RootElement;

        internal Match(MatchElement rootElement)
        {
            RootElement = rootElement;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[Match]" + RootElement.ToString();
        }
    }
}
