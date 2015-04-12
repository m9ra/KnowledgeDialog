using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.TaskPatterns
{
    class WifeOfPresidentTask : TaskPatternBase
    {
        internal WifeOfPresidentTask(ComposedGraph graph)
            : base(graph)
        {
            SetPattern("Check if system can search name of wife of {0} president." + TaskPatternUtilities.CheckAndLearn);

            Substitutions(TaskPatternUtilities.StateSubstitutions);
            ExpectedAnswerRule(TaskPatternUtilities.WifeOfPresidentFromStatePath); 
        }
    }
}
