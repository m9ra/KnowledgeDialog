using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.PoolComputation.MappedQA.Features;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class RankedInterpretations
    {
        private Dictionary<Tuple<FeatureCover, RuledInterpretation>, double> _interpretationScores = new Dictionary<Tuple<FeatureCover, RuledInterpretation>, double>();

        internal Ranked<Tuple<FeatureCover,RuledInterpretation>> BestMatch
        {
            get
            {
                var bestPair = _interpretationScores.OrderByDescending(prop => prop.Value).FirstOrDefault();

                return new Ranked<Tuple<FeatureCover,RuledInterpretation>>(bestPair.Key, bestPair.Value);
            }
        }

        internal void AddInterpretation(FeatureCover cover,RuledInterpretation interpretation, double rank)
        {
            double score;
            var key = Tuple.Create(cover, interpretation);
            _interpretationScores.TryGetValue(key, out score);
            score += rank;
            _interpretationScores[key] = score;
        }
    }
}
