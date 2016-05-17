using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class Welcome : StateBase
    {
        protected override ModifiableResponse execute()
        {
            return Response("Welcome, how can I help you?");
        }
    }
}
