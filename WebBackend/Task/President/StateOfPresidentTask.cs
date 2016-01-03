using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Task.President
{
    class StateOfPresidentTask : TaskPatternBase
    {
        internal StateOfPresidentTask(ComposedGraph graph)
            : base(graph)
        {

            SetPattern("Check if system can search <b>name of the president of {0}</b>." + TaskPatternUtilities.CheckAndLearn);

            TaskPatternUtilities.FillPresidentTask(this,
                president => president.State,
                president => president.Name
         );
        }
    }
}
