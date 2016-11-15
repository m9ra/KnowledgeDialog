using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.GeneralizationQA
{
    class TraceNodeFollower
    {
        public readonly TraceNode CurrentNode;

        public readonly IEnumerable<NodeReference> NodeLayer;
    }
}
