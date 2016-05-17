using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class InterpretationCounter
    {
        internal RuledInterpretation BestInterpretation { get; private set; }

        internal double BestInterpretationRank { get; private set; }

        internal double BestCount { get { return _maxCount; } }

        private readonly Dictionary<RuledInterpretation, int> _ruleCounts = new Dictionary<RuledInterpretation, int>();

        private int _maxCount = 0;

        private int _totalInterpretationCount = 0;

        internal void Add(RuledInterpretation interpretation)
        {
            ++_totalInterpretationCount;

            int count;
            _ruleCounts.TryGetValue(interpretation, out count);
            ++count;

            //set new value
            _ruleCounts[interpretation] = count;

            if (count > _maxCount)
            {
                //better interpretation has been found
                _maxCount = count;
                BestInterpretation = interpretation;
            }

            BestInterpretationRank = 1.0 * _maxCount / _totalInterpretationCount;
        }
    }
}
