using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class RuleHead : RuleBitBase
    {
        internal readonly string HeadCaption;

        internal RuleHead(string headCaption)
        {
            HeadCaption = headCaption;
        }

        /// <inheritdoc/>
        protected override string getNotation()
        {
            return "(" + HeadCaption + ")";
        }
    }
}
