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
        private readonly FeatureMapping _mapping;


        internal MappedQAModule(ComposedGraph graph, CallStorage storage)
            : base(graph, storage)
        {
            _mapping = new FeatureMapping(graph);
        }

        #region Template methods

        protected override bool adviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            var parsedQuestion = UtteranceParser.Parse(question);

            var interpretationsFactory = getInterpretationsFactory(parsedQuestion, isBasedOnContext, correctAnswerNode, context);

            var covers = FeatureCover.GetFeatureCovers(parsedQuestion, Graph);

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
            var covers = FeatureCover.GetFeatureCovers(expression, Graph);

            var allRankedMappings = new List<Ranked<ContextRuleMapping>>();
            foreach (var cover in covers)
            {
                var rankedMappings = _mapping.GetRankedMappings(cover);
                allRankedMappings.AddRange(rankedMappings);
            }

            throw new NotImplementedException();
        }

        internal void Optimize()
        {
            _mapping.Optimize();
        }

        #region Mapping creation

        private InterpretationsFactory getInterpretationsFactory(ParsedUtterance parsedQuestion, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            return new InterpretationsFactory(parsedQuestion, isBasedOnContext, correctAnswerNode);
        }


        #endregion



    }
}
