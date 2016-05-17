using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class PoolHypothesis
    {
        public readonly NodesSubstitution Substitutions;

        public readonly ActionBlock ActionBlock;

        public readonly MappingControl<ActionBlock> Control;

        public PoolHypothesis(NodesSubstitution substitutions, MappingControl<ActionBlock> control)
        {
            Substitutions = substitutions;
            ActionBlock = control.Value;
            Control = control;
        }
    }
}
