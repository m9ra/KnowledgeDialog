﻿using System;
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
            SetPattern("Check if system can search <b>name of wife of the president of {0}</b>." + TaskPatternUtilities.CheckAndLearn);

            TaskPatternUtilities.FillPresidentTask(this,
                president => president.Name,
                president => president.WifeName
            );
        }
    }
}
