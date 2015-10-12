using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class ProbabilisticMapping
    {
        /// <summary>
        /// Feature to interpretation counter.
        /// </summary>
        private readonly Dictionary<FeatureCover, InterpretationCounter> _coverIndex = new Dictionary<FeatureCover, InterpretationCounter>();

        internal void ReportInterpretation(FeatureCover cover, RuledInterpretation interpretation)
        {
            InterpretationCounter counter;
            if (!_coverIndex.TryGetValue(cover, out counter))
                //create new counter
                _coverIndex[cover] = counter = new InterpretationCounter();

            counter.Add(interpretation);
        }


        internal RuledInterpretation GetBestInterpretation(FeatureCover feature)
        {
            InterpretationCounter counter;
            if (!_coverIndex.TryGetValue(feature, out counter))
                return null;

            return counter.BestInterpretation;
        }
    }
}
