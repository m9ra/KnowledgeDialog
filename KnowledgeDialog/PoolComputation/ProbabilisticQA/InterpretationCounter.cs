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

        private readonly Dictionary<RuledInterpretation, int> _ruleCounts = new Dictionary<RuledInterpretation, int>();

        private int _maxCount = 0;

        internal void Add(RuledInterpretation interpretation)
        {
            int count;
            _ruleCounts.TryGetValue(interpretation, out count);
            ++count;

            //set new value
            _ruleCounts[interpretation] = count + 1;

            if (count > _maxCount)
            {
                //better interpretation has been found
                _maxCount = count;
                BestInterpretation = interpretation;
            }
        }
    }
}
