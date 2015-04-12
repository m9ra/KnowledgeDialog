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
        internal StateOfPresidentTask(ComposedGraph graph)
            : base(graph)
        {

            SetPattern("Check if system can search name of president of {0}." + TaskPatternUtilities.CheckAndLearn);

            Substitutions(TaskPatternUtilities.StateSubstitutions);
            ExpectedAnswerRule(TaskPatternUtilities.ReignsInFromStatePath);
        }
    }
}
