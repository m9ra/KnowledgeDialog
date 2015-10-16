using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class RankedInterpretations
    {
        private Dictionary<RuledInterpretation, double> _interpretationScores = new Dictionary<RuledInterpretation, double>();

        internal Ranked<RuledInterpretation> BestInterpretation
        {
            get
            {
                var bestPair = _interpretationScores.OrderByDescending(prop => prop.Value).FirstOrDefault();

                return new Ranked<RuledInterpretation>(bestPair.Key, bestPair.Value);
            }
        }

        internal void AddInterpretation(RuledInterpretation interpretation, double rank)
        {
            double score;
            _interpretationScores.TryGetValue(interpretation, out score);
            score += rank;
            _interpretationScores[interpretation] = score;
        }
    }
}
