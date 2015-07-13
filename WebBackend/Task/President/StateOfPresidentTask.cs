﻿using System;
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

            SetPattern("Check if system can search <b>name of president of {0}</b>." + TaskPatternUtilities.CheckAndLearn);

            TaskPatternUtilities.FillPresidentTask(this,
                president => president.State,
                president => president.Name
         );
        }
    }
}