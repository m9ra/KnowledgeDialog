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
        private readonly Dictionary<FeatureKey, InterpretationCounter> _coverIndex = new Dictionary<FeatureKey, InterpretationCounter>();

        /// <summary>
        /// How many interpretations has been registered.
        /// </summary>
        internal int RegisteredInterpretations { get; private set; }

        /// <summary>
        /// How many feature keys has been registered.
        /// </summary>
        internal int RegisteredFeatureCount { get { return _coverIndex.Count; } }

        internal void ReportInterpretation(FeatureKey key, RuledInterpretation interpretation)
        {
            InterpretationCounter counter;
            if (!_coverIndex.TryGetValue(key, out counter))
                //create new counter
                _coverIndex[key] = counter = new InterpretationCounter();

            counter.Add(interpretation);
            ++RegisteredInterpretations;
        }

        internal Ranked<RuledInterpretation> GetRankedInterpretation(FeatureCover cover)
        {
            InterpretationCounter counter;
            if (!_coverIndex.TryGetValue(cover.CreateFeatureKey(), out counter))
                return null;

            return new Ranked<RuledInterpretation>(counter.BestInterpretation, counter.BestInterpretationRank);
        }

        internal RankedInterpretations GetRankedInterpretations(IEnumerable<FeatureCover> covers)
        {
            var result = new RankedInterpretations();
            foreach (var cover in covers)
            {
                var rankedInterpretation = GetRankedInterpretation(cover);
                if (rankedInterpretation == null)
                    //we don't have interpretation fot the cover
                    continue;
                result.AddInterpretation(rankedInterpretation.Value, rankedInterpretation.Rank);
            }

            return result;
        }
    }
}
