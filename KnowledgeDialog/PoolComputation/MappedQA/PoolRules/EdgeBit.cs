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
        internal readonly string Edge;

        internal readonly bool IsOutgoing;

        internal EdgeBit(string edge, bool isOutgoing)
        {
            if (edge == null)
                throw new ArgumentNullException("edge");

            Edge = edge;
            IsOutgoing = IsOutgoing;
        }

        /// <inheritdoc/>
        protected override string getNotation()
        {
            if (IsOutgoing)
                return "--" + Edge + "->";
            else
                return "<-" + Edge + "--";
        }
    }
}
