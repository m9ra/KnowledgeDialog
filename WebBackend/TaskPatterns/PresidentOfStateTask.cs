﻿using System;
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
            SetPattern("Check if system can search state which president is {0}." + TaskPatternUtilities.CheckAndLearn);

            Substitutions(TaskPatternUtilities.PresidentSubstitutions);
            ExpectedAnswerRule(TaskPatternUtilities.ReignsInFromPresidentPath);
        }
    }
}
