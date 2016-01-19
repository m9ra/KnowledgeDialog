using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class EdgeBit : RuleBitBase
    {
        internal readonly Edge Edge;

        internal readonly bool IsOutgoing;

        internal EdgeBit(Edge edge)
        {
            if (edge == null)
                throw new ArgumentNullException("edge");

            Edge = edge;
        }

        /// <inheritdoc/>
        protected override string getNotation()
        {
            var edgeName = Edge.Name;
            if (Edge.IsOutcoming)
                return "--" + edgeName + "->";
            else
                return "<-" + edgeName + "--";
        }
    }
}
