using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Task.President
{
    class SonOfTask: TaskPatternBase
    {
        internal SonOfTask(ComposedGraph graph)
            : base(graph)
        {
            SetPattern("Check if system can search some <b>son of {0}</b>." + TaskPatternUtilities.CheckAndLearn);

            TaskPatternUtilities.FillPresidentTask(this,
                president => president.Name,
                president => president.SonName
                    );
        }
    }
}
