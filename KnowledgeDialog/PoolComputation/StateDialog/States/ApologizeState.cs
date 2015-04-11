﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class ApologizeState : StateBase
    {
        protected override ModifiableResponse execute()
        {
            EmitEdge(ForwardEdge);

            return Response("I'm sorry, give me another question please.");
        }
    }
}
