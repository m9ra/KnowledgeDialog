using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    delegate void TriggerAction(StateContext context);


    class Trigger
    {
        internal readonly StateGraphBuilder TargetNode;

        private readonly TriggerAction _action;

        internal Trigger(StateGraphBuilder targetNode, TriggerAction action)
        {
            TargetNode = targetNode;
            _action = action;
        }

        internal void Apply(StateContext context)
        {
            if (_action != null)
                _action(context);
        }
    }
}
