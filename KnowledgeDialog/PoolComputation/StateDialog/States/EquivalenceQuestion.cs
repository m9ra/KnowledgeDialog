using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog.States
{
    class EquivalenceQuestion : StateBase
    {
        public static readonly EdgeIdentifier EquivalenceEdge = new EdgeIdentifier();

        public static readonly StateProperty PatternQuestion = new StateProperty();

        public static readonly StateProperty QueriedQuestion = new StateProperty();

        protected override ModifiableResponse execute()
        {
            var patternQuestion = Context.Get(PatternQuestion);
            var queriedQuestion = Context.Get(QueriedQuestion);

            return Response("Is your question same as '"+patternQuestion+"'?");
        }
    }
}
