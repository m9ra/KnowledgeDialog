using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class NodeBit : RuleBitBase
    {
        internal readonly NodeReference Node;

        internal NodeBit(NodeReference node)
        {
            Node = node;
        }

        /// <inheritdoc/>
        protected override string getNotation()
        {
            return Node.ToString();
        }
    }
}
