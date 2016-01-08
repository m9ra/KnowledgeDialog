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

        /// <summary>
        /// Index of generator and feature covers indexed by feature keys.
        /// </summary>
        private readonly Dictionary<FeatureKey, List<Tuple<InterpretationGenerator, FeatureCover>>> _keyToGeneratorCovers = new Dictionary<FeatureKey, List<Tuple<InterpretationGenerator, FeatureCover>>>();

        /// <summary>
        /// Interpretations that were 
        /// </summary>
        private readonly HashSet<Interpretation> _completedGeneralInterpretations = new HashSet<Interpretation>();

        internal ProbabilisticQAModule(ComposedGraph graph, CallStorage storage)
            : base(graph, storage)
        {
        }

        internal IEnumerable<NodeReference> GetAnswer(string question, ContextPool pool)
        {
            return GetRankedAnswer(question, pool).Value;
        }

        internal Ranked<IEnumerable<NodeReference>> GetRankedAnswer(string question, ContextPool pool)
        {
            //keep original pool non-modified
            pool = pool.Clone();

            var parsedQuestion = UtteranceParser.Parse(question);
            var covers = FeatureCover.GetFeatureCovers(parsedQuestion, Graph);

            var interpretations = _mapping.GetRankedInterpretations(covers);
            var bestMatch = interpretations.BestMatch;
            if (bestMatch.Value == null)
                //no interpretation has been found
                return new Ranked<IEnumerable<NodeReference>>(new NodeReference[0], 0.0);

            var bestInterpretation = bestMatch.Value.Item2.Interpretation;
            var bestCover = bestMatch.Value.Item1;
            var instantiatedInterpretation = bestInterpretation.InstantiateBy(bestCover, Graph);

            //run the interpretation on the pool
            Console.WriteLine("\n");
            foreach (var rule in instantiatedInterpretation.Rules)
            {
                rule.Execute(pool);
                Console.WriteLine(rule);
                Console.WriteLine("POOL: " + string.Join(" ", pool.ActiveNodes));
            }

            return new Ranked<IEnumerable<NodeReference>>(pool.ActiveNodes, bestMatch.Rank);
        }

        /// <summary>
        /// Starts optimization routine of the module for given number of steps.
        /// </summary>
        /// <param name="stepCount">Number of steps to optimize if positive, otherwise infinite number of steps is applied.</param>
        internal void Optimize(int stepCount)
        {
            var currentStep = 0;
            while (currentStep < stepCount || stepCount < 0)
            {
                //TODO handle no next interpretation situations
                for (var i = 0; i < _interpretationGenerators.Count; ++i)
                {
                    var generator = _interpretationGenerators[i];
                    var interpretation = generator.GetNextInterpretation();
                    if (interpretation != null)
                        //report interpretation for generator covers
                        reportNextIntepretation(interpretation, generator.Covers);

                    ++currentStep;
                }
            }
        }

        private void reportNextIntepretation(Interpretation interpretation, IEnumerable<FeatureCover> reportingCovers)
        {
            //check all generators for compatibility with given interpretation
            foreach (var reportingCover in reportingCovers)
            {
                foreach (var generalInterpretation in getGeneralInterpretations(interpretation, reportingCover))
                {
                    reportToCompatibleGenerators(reportingCover, generalInterpretation);
                }
            }
        }

        private IEnumerable<Interpretation> getGeneralInterpretations(Interpretation interpretation, FeatureCover reportingCover)
        {
            var generalInterpretation = interpretation.GeneralizeBy(reportingCover, Graph);
            if (generalInterpretation != null)
            {
                //report interpretation as is
                yield return generalInterpretation;
                yield break;
            }

            foreach (var extendedInterpretation in interpretation.ExtendBy(reportingCover.GetNodes(Graph), Graph))
            {
                var generalExtendedInterpretation = extendedInterpretation.GeneralizeBy(reportingCover, Graph);

                if (generalExtendedInterpretation == null)
                    throw new NotSupportedException("generalization should be successful");
                yield return generalExtendedInterpretation;
            }
        }

        private void reportToCompatibleGenerators(FeatureCover reportingCover, Interpretation generalInterpretation)
        {
            if (_completedGeneralInterpretations.Contains(generalInterpretation))
                //we are searchnig only for new general interpretations
                return;

            //remember that we have proceed this interpretation so we don't need to repeatedly increment it
            _completedGeneralInterpretations.Add(generalInterpretation);

            //feature key has always been present in the index
            var generatorCovers = _keyToGeneratorCovers[reportingCover.FeatureKey];

            var compatibleCovers = new List<FeatureCover>();
            foreach (var generatorCover in generatorCovers)
            {
                var generator = generatorCover.Item1;
                var cover = generatorCover.Item2;

                var instantiatedInterpretation = generalInterpretation.InstantiateBy(cover, Graph);
                if (isCompatibleInterpretation(instantiatedInterpretation, generator))
                {
                    compatibleCovers.Add(cover);
                    var ruledInterpretation = new RuledInterpretation(generalInterpretation, reportingCover.FeatureKey);
                    _mapping.ReportInterpretation(ruledInterpretation);
                }
            }

            if (compatibleCovers.Count > 1)
            {
                Console.WriteLine(generalInterpretation);
                Console.WriteLine("\tCOMPATIBLE WITH:");
                foreach (var cover in compatibleCovers)
                {
                    var origin = cover.FeatureInstances.First().Origin;
                    Console.WriteLine("\t\t" + origin);
                }

                Console.WriteLine();
            }
        }

        private bool isCompatibleInterpretation(Interpretation instantiatedInterpretation, InterpretationGenerator generator)
        {
            var contextPool = generator.CreateContextPoolCopy();
            foreach (var rule in instantiatedInterpretation.Rules)
            {
                rule.Execute(contextPool);
            }

            return generator.ContainsCorrectAnswer(contextPool);
        }

        #region Template methods implementation

        protected override bool adviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            var parsedQuestion = UtteranceParser.Parse(question);
            var covers = FeatureCover.GetFeatureCovers(parsedQuestion, Graph);

            //setup interpretation generator
            var factory = new InterpretationsFactory(parsedQuestion, isBasedOnContext, correctAnswerNode);

            var contextPool = new ContextPool(Graph);
            contextPool.Insert(context.ToArray());

            var generator = new InterpretationGenerator(covers, factory, contextPool, this);
            //!!!!!!!!!!!!!!TODOOOOOOOOOOOOO!!!!!!!!!!!!!! 
            //We have to check compatibility of the generator with all known interpretations
            _interpretationGenerators.Add(generator);

            //register covers according to its feature keys
            foreach (var cover in generator.Covers)
            {
                List<Tuple<InterpretationGenerator, FeatureCover>> generators;
                if (!_keyToGeneratorCovers.TryGetValue(cover.FeatureKey, out generators))
                    _keyToGeneratorCovers[cover.FeatureKey] = generators = new List<Tuple<InterpretationGenerator, FeatureCover>>();

                generators.Add(Tuple.Create(generator, cover));
            }

            //TODO decide whether it would be benefitial to report that
            //the advice is taken into account, however we don't believe it much.

            //TODO initialize with first interpretation?
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
    }
}
