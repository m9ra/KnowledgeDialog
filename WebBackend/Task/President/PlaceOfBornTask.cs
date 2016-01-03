using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Task.President
{
    class PlaceOfBornTask: TaskPatternBase
    {
        internal PlaceOfBornTask(ComposedGraph graph)
            : base(graph)
        {
            SetPattern("Check if system can search <b>place where {0} was born</b>." + TaskPatternUtilities.CheckAndLearn);

            TaskPatternUtilities.FillPresidentTask(this,
                president => president.Name,
                president => president.PlaceOfBorn
                    );
        }
    }
}
