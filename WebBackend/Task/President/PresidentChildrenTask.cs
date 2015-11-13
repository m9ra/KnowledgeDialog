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
            SetPattern("Check if system can search some <b>child of president {0}</b>." + TaskPatternUtilities.CheckAndLearn);

            TaskPatternUtilities.FillPresidentTask(this,
                president => president.Name,
                president => president.Children
                    );
        }
    }
}
