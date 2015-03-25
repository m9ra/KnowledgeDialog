using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class HowCanIHelp:StateBase
    {
        protected override ModifiableResponse execute()
        {
            EmitEdge(ForwardEdge);
            return Response("How can I help you?");
        }
    }
}
