using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    public class EvaluationContext
    {
        private readonly Dictionary<KnowledgeGroup, IEvaluation> _evaluations = new Dictionary<KnowledgeGroup, IEvaluation>();

        public readonly ComposedGraph Graph;

        public bool IsEmpty { get { return _evaluations.Count == 0; } }

        internal EvaluationContext(IEnumerable<KnowledgeGroup> groups, ComposedGraph graph)
        {
            Graph = graph;
            var evaluations = (from grp in groups select Tuple.Create(grp, new PartialMatching.PartialEvaluation(grp, graph))).ToArray();

            foreach (var evaluation in evaluations)
            {
                _evaluations[evaluation.Item1] = evaluation.Item2;
            }
        }

        internal IEnumerable<WeightedPattern> GetSortedPatterns(IEnumerable<WeightedPattern> patterns)
        {
            var scoredPatterns = GetScoredPatterns(patterns);

            return scoredPatterns.OrderByDescending(t => t.Item2).Select(t => t.Item1);
        }


        internal List<Tuple<WeightedPattern, double>> GetScoredPatterns(IEnumerable<WeightedPattern> patterns)
        {
            var patternScores = new List<Tuple<WeightedPattern, double>>();
            foreach (var pattern in patterns)
            {
                var score = GetScore(pattern);
                var t = Tuple.Create(pattern, score);
                patternScores.Add(t);
            }

            return patternScores;
        }

        internal double GetScore(WeightedPattern pattern)
        {
            var score = 0.0;
            foreach (var feature in pattern.Features)
            {
                var weight = pattern.GetWeight(feature);
                var featureScore = GetScore(feature);

                if (featureScore < 0)
                    //feature is not matched
                    return 0;

                score += weight * featureScore;
            }

            return score;
        }

        internal double GetScore(PathFeature feature)
        {
            //TODO here could be caching
            var groupEvaluation = _evaluations[feature.ContainingGroup];

            return GetScore(feature, Graph, groupEvaluation);
        }

        internal static double GetScore(PathFeature feature, ComposedGraph context, IEvaluation evaluation)
        {
            var distance = 0;
            var count = 0;
            foreach (var node in feature.Path.Nodes)
            {
                var substitution = evaluation.GetSubstitution(node);
                ++count;

                var path = context.GetPaths(node, substitution, 1000, 1000).FirstOrDefault();

                if (substitution == null || path == null)
                    //feature is not activated
                    return -1;

                distance += path.Length;
            }

            return 1.0 * count / (count + distance);
        }

        internal NodeReference GetSubstitution(NodeReference node, KnowledgeGroup contextGroup)
        {
            var evaluation = _evaluations[contextGroup];
            return evaluation.GetSubstitution(node);
        }

        internal object GetScore(KeyValuePair<PathFeature, double> feature)
        {
            throw new NotImplementedException();
        }
    }
}
