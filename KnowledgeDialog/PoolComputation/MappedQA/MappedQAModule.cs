using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;
using KnowledgeDialog.Database;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA
{
    class MappedQAModule : QuestionAnsweringModuleBase
    {
        private readonly List<FeatureGeneratorBase> _generators = new List<FeatureGeneratorBase>();

        private readonly FeatureMapping _mapping = new FeatureMapping();


        internal MappedQAModule(ComposedGraph graph, CallStorage storage)
            : base(graph, storage)
        {
        }

        #region Template methods

        protected override bool adviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            var parsedQuestion = UtteranceParser.Parse(question);

            var interpretationsFactory = getInterpretationsFactory(parsedQuestion, isBasedOnContext, correctAnswerNode, context);

            var features = createFeatures(parsedQuestion);
            var covers = getFeatureCovers(parsedQuestion);
            _mapping.Add(interpretationsFactory, covers);

            //TODO decide whether it would be benefitial to report that
            //the advice is taken into account, however we don't believe it much.
            return true;
        }

        protected override void repairAnswer(string question, NodeReference suggestedAnswer, IEnumerable<NodeReference> context)
        {
            throw new NotImplementedException();
        }

        protected override void setEquivalence(string patternQuestion, string queriedQuestion, bool isEquivalent)
        {
            throw new NotImplementedException();
        }

        protected override void negate(string question)
        {
            throw new NotImplementedException();
        }

        #endregion

        internal NodeReference GetAnswer(ParsedUtterance expression)
        {
            var features = createFeatures(expression);
            var covers = getFeatureCovers(expression);

            var allScoredMappings = new List<ScoredRuleMapping>();
            foreach (var cover in covers)
            {
                var scoredMappings = _mapping.GetScoredMappings(cover);
                allScoredMappings.AddRange(scoredMappings);
            }

            throw new NotImplementedException();
        }

        #region Mapping creation

        private InterpretationsFactory getInterpretationsFactory(ParsedUtterance parsedQuestion, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            return new InterpretationsFactory(parsedQuestion, isBasedOnContext, correctAnswerNode, context);
        }


        #endregion

        #region Expression mapping


        private IEnumerable<FeatureInstance> createFeatures(ParsedUtterance expression)
        {
            var features = new HashSet<FeatureInstance>();
            foreach (var generator in _generators)
            {
                features.UnionWith(generator.GenerateFeatures(expression));
            }

            return features;
        }

        private IEnumerable<FeatureCover> getFeatureCovers(ParsedUtterance expression)
        {
            var features = createFeatures(expression);
            var index = new FeatureIndex(features);

            return generateCovers(index, 0);
        }

        private IEnumerable<FeatureCover> generateCovers(FeatureIndex index, int currentPosition)
        {
            if (index.Length == 0)
                //there are no possible covers
                return new FeatureCover[0];

            if (index.Length == currentPosition)
            {
                //we are at bottom of recursion
                //we will initiate covers that will be extended later through the recursion

                var newCovers=new List<FeatureCover>();
                foreach (var feature in index.GetFeatures(currentPosition))
                {
                    var cover = new FeatureCover(feature);
                    newCovers.Add(cover);
                }

                return newCovers;
            }

            //extend covers that we got from deeper recursion
            var previousCovers = generateCovers(index, currentPosition + 1);
            var extendedCovers = new List<FeatureCover>();
            foreach (var cover in previousCovers)
            {
                foreach (var feature in index.GetFeatures(currentPosition))
                {
                    extendedCovers.AddRange(cover.Extend(feature));
                }
            }

            return extendedCovers;
        }

        #endregion
    }
}
