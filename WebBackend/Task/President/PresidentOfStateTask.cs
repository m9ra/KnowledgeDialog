using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.TaskPatterns
{
    class PresidentOfStateTask : TaskPatternBase
    {
        internal PresidentOfStateTask(ComposedGraph graph)
            : base(graph)
        {
            SetPattern("Check if system can search <b>state where {0} is the president.</b>" + TaskPatternUtilities.CheckAndLearn);

            TaskPatternUtilities.FillPresidentTask(this,
                president => president.Name,
                president => president.State 
                    );
        }
    }
}
