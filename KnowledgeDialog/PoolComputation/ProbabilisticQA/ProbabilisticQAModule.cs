using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.Features;
using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class ProbabilisticQAModule : QuestionAnsweringModuleBase
    {
        /// <summary>
        /// Probabilistic mapping.
        /// </summary>
        private readonly ProbabilisticMapping _mapping = new ProbabilisticMapping();

        /// <summary>
        /// Generators that are used for <see cref="_mapping"/> filling.
        /// </summary>
        private readonly List<InterpretationGenerator> _interpretationGenerators = new List<InterpretationGenerator>();

        internal ProbabilisticQAModule(ComposedGraph graph, CallStorage storage)
            : base(graph, storage)
        {
        }

        #region Template methods implementation

        protected override bool adviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            var parsedQuestion = UtteranceParser.Parse(question);
            var covers = FeatureCover.GetFeatureCovers(parsedQuestion);

            //setup interpretation generator
            var factory = new InterpretationsFactory(parsedQuestion, isBasedOnContext, correctAnswerNode, context);
            var generator = new InterpretationGenerator(covers, factory, this);
            _interpretationGenerators.Add(generator);


            //TODO decide whether it would be benefitial to report that
            //the advice is taken into account, however we don't believe it much.

            //initialize with first interpretation
            return reportNextIntepretation(generator);
        }

        protected override void repairAnswer(string question, Knowledge.NodeReference suggestedAnswer, IEnumerable<NodeReference> context)
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

        internal void Optimize()
        {
            while (true)
            {
                //TODO handle no next interpretation situations
                for (var i = 0; i < _interpretationGenerators.Count; ++i)
                {
                    var generator = _interpretationGenerators[i];
                    reportNextIntepretation(generator);
                }
            }
        }

        private bool reportNextIntepretation(InterpretationGenerator generator)
        {
            var interpretation = generator.GetNextInterpretation();
            if (interpretation == null)
                //no other interpretation available from generator
                return false;

            foreach (var cover in generator.Covers)
            {
                var ruledInterpretation =  new RuledInterpretation(interpretation, cover);
                _mapping.ReportInterpretation(cover, ruledInterpretation);
            }

            return true;
        }
    }
}
