using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class RuledInterpretation
    {
        /// <summary>
        /// Interpretation containing rule.
        /// </summary>
        private readonly Interpretation _interpretation;

        /// <summary>
        /// Cover which belongs to interpretation
        /// </summary>
        private readonly FeatureCover _cover;

        public RuledInterpretation(Interpretation interpretation, FeatureCover cover)
        {
            _interpretation = interpretation;
            _cover = cover;
        }
    }
}
