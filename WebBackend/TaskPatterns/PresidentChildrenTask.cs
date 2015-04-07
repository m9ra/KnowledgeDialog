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
        PresidentChildrenTask(ComposedGraph graph)
            : base(graph)
        {
        }
    }
}
