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

        public static readonly StateProperty2 PatternQuestion = new StateProperty2();

        public static readonly StateProperty2 QueriedQuestion = new StateProperty2();

        protected override ModifiableResponse execute()
        {
            var patternQuestion = Context.Get(PatternQuestion);
            var queriedQuestion = Context.Get(QueriedQuestion);

            return Response("Is your question same as '"+patternQuestion+"'?");
        }
    }
}
