using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class SemanticItem
    {
        public readonly string Question;

        public readonly string Answer;

        public readonly Constraints Constraints;

    }

    class Constraints
    {
        public readonly string Input;

        public readonly IEnumerable<string> Conditions;
    }
}
