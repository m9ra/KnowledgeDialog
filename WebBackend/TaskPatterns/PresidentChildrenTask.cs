using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.TaskPatterns
{
    class PresidentChildrenTask : TaskPatternBase
    {
        internal PresidentChildrenTask(ComposedGraph graph)
            : base(graph)
        {
            SetPattern("Check if system can search some child of president of {0}." + TaskPatternUtilities.CheckAndLearn);

            Substitutions(TaskPatternUtilities.StateSubstitutions);
            ExpectedAnswerRule(TaskPatternUtilities.PresidentChildFromState);
        }
    }
}
