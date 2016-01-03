using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Task.President
{
    class PresidentMotherTask: TaskPatternBase
    {
        internal PresidentMotherTask(ComposedGraph graph)
            : base(graph)
        {
            SetPattern("Check if system can search <b>mother of {0}</b>." + TaskPatternUtilities.CheckAndLearn);

            TaskPatternUtilities.FillPresidentTask(this,
                president => president.Name,
                president => president.MotherName
                    );
        }
    }
}
