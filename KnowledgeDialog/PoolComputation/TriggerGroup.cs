using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.StateDialog;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class TriggerGroup : UtteranceMapping<Trigger>
    {
        internal TriggerGroup(ComposedGraph graph)
            : base(graph)
        {
        }

        internal void FillFrom(TriggerGroup triggerGroup)
        {
            foreach (var pair in triggerGroup.Mapping)
            {
                this.Mapping.Add(pair.Key, pair.Value);
            }
        }
    }
}
