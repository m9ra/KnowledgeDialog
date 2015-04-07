using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.TaskPatterns
{
    class StateOfPresidentTask : TaskPatternBase
    {
        public StateOfPresidentTask(ComposedGraph graph)
            : base(graph)
        {
            SetPattern("Check if system can search name of president of {0}. If not, try to teach it to the system.");

            Substitutions(
                "United states of America",
                "Mexico",
                "Germany",
                "Russia"
                );

            ExpectedAnswerRule("reigns in", false);
        }
    }
}
