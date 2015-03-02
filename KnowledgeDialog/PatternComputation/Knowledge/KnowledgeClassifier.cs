using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    public class KnowledgeClassifier<ClassType>
    {
        /// <summary>
        /// Knowledge that is used for searching differences between classes.
        /// </summary>
        public readonly ComposedGraph Knowledge;

        /// <summary>
        /// Theses classifications are considered as ground truth knowledge.
        /// </summary>
        private readonly Dictionary<NodeReference, ClassType> _knownClassifications = new Dictionary<NodeReference, ClassType>();

        /// <summary>
        /// Current tree of rules that are used for classification.
        /// </summary>
        internal KnowledgeRule Root { get; private set; }

        public KnowledgeClassifier(ComposedGraph knowledge)
        {
            Knowledge = knowledge;
        }

        /// <summary>
        /// Classify given node into class.
        /// </summary>
        /// <param name="node">Classified node.</param>
        /// <returns>Best class that fits the node.</returns>
        public ClassType Classify(NodeReference node)
        {
            ClassType classification;
            if (_knownClassifications.TryGetValue(node, out classification))
                //we know this example
                return classification;

            return classify(node, Root);
        }

        /// <summary>
        /// Advice class presence for given node.
        /// </summary>
        /// <param name="node">Node which classification is improved.</param>
        /// <param name="cls">Adviced class.</param>
        public void Advice(NodeReference node, ClassType cls)
        {
            _knownClassifications[node] = cls;
            retrain();
        }

        /// <summary>
        /// Negate class presence for given node.
        /// </summary>
        /// <param name="node">Node which classification is improved.</param>
        /// <param name="cls">Negated class.</param>
        public void Negate(NodeReference node, ClassType cls)
        {
            throw new NotImplementedException();
        }

        #region Classification utilities

        private ClassType classify(NodeReference node, KnowledgeRule rule)
        {
            if (rule.Path == null)
                //there is no constraint
                return getKnownClassifications(_knownClassifications.Keys.First());


            var layer = Knowledge.GetForwardTargets(new[] { node }, rule.Path);

            var isSatisfied = layer.Contains(rule.EndNode);
            if (isSatisfied && rule.YesRule != null)
                return classify(node, rule.YesRule);

            if (!isSatisfied && rule.NoRule != null)
                return classify(node, rule.NoRule);

            var classExamples = isSatisfied ? rule.InitialYesNodes : rule.InitialNoNodes;
            var opositeExamples = isSatisfied ? rule.InitialNoNodes : rule.InitialYesNodes;

            if (!classExamples.Any())
                classExamples = opositeExamples;

            //TODO examples could be missing
            return getKnownClassifications(classExamples.First());
        }

        #endregion

        #region Training utilities

        private void retrain()
        {
            var log = new MultiTraceLog(_knownClassifications.Keys, Knowledge);
            Root = createRuleTree(new HashSet<NodeReference>(_knownClassifications.Keys), log);
        }

        private IEnumerable<NodeReference> getCoverage(TraceNode node)
        {
            var bestTrace = getCoverageTrace(node);
            if (bestTrace == null)
                return Enumerable.Empty<NodeReference>();

            return bestTrace.InitialNodes;
        }

        private static Trace getCoverageTrace(TraceNode node)
        {
            var maxSize = int.MinValue;
            Trace bestTrace = null;
            foreach (var trace in node.Traces)
            {
                var currentSize = trace.InitialNodes.Count();
                if (currentSize > maxSize)
                {
                    maxSize = currentSize;
                    bestTrace = trace;
                }
            }
            return bestTrace;
        }

        private KnowledgeRule createRuleTree(IEnumerable<NodeReference> toCover, MultiTraceLog log)
        {
            var rule = createBestRule(toCover, log);

            if (
                rule.InitialYesNodes.Count() == 0 ||
                rule.InitialNoNodes.Count() == 0
                )

                //current rule doesn't provide any improvement
                return rule;


            if (!isClassConsistent(rule.InitialNoNodes))
                rule.AddNoBranch(createRuleTree(rule.InitialNoNodes, log));

            if (!isClassConsistent(rule.InitialYesNodes))
                rule.AddYesBranch(createRuleTree(rule.InitialYesNodes, log));

            return rule;
        }

        private KnowledgeRule createBestRule(IEnumerable<NodeReference> classifiedNodes, MultiTraceLog log)
        {
            var bestScore = Double.NegativeInfinity;
            TraceNode bestTraceNode = null;
            Trace bestTrace = null;
            IEnumerable<NodeReference> bestCoverage = null;

            //find best classification rule
            foreach (var traceNode in log.TraceNodes)
            {
                foreach (var trace in traceNode.Traces)
                {
                    var coverage = trace.InitialNodes;
                    var currentScore = getClassificationScore(classifiedNodes, coverage, trace);
                    if (currentScore > bestScore)
                    {
                        bestCoverage = coverage;
                        bestScore = currentScore;
                        bestTraceNode = traceNode;
                        bestTrace = trace;
                    }
                }
            }

            var yesNodes = bestCoverage.Intersect(classifiedNodes);
            var noNodes = classifiedNodes.Except(yesNodes);

            return new KnowledgeRule(bestTraceNode.Path, bestTrace.CurrentNode, yesNodes, noNodes);
        }

        private double getClassificationScore(IEnumerable<NodeReference> classifiedNodes, IEnumerable<NodeReference> coverage, Trace trace)
        {
            var yesCounts = getClassCounts(coverage);
            var noCounts = getClassCounts(classifiedNodes.Except(coverage));

            var discount = 0;
            foreach (var cls in yesCounts.Keys)
            {
                if (!noCounts.ContainsKey(cls))
                    //class is separated clearly
                    continue;

                discount += Math.Min(yesCounts[cls], noCounts[cls]);
            }

            var totalYesCounts = getTotalCounts(yesCounts);
            var totalNoCounts = getTotalCounts(noCounts);

            var binaryDiscount = 1.0 * Math.Abs(totalYesCounts - totalNoCounts) / Math.Abs(totalYesCounts + totalNoCounts);
            if (_knownClassifications.Keys.Contains(trace.CurrentNode))
                //prevent node overfitting
                binaryDiscount += 0.5;

            //prefer more generalizing rules
            var magnitude = Knowledge.GetNeighbours(trace.CurrentNode, 10).Count() / 10.0;

            return -discount - binaryDiscount;
        }

        private bool isClassConsistent(IEnumerable<NodeReference> nodes)
        {
            if (!nodes.Any())
                //there is no node that could be inconsistent
                return true;

            var requiredClass = getKnownClassifications(nodes.First());

            return nodes.All(node => requiredClass.Equals(getKnownClassifications(node)));
        }

        private Dictionary<ClassType, int> getClassCounts(IEnumerable<NodeReference> nodes)
        {
            var result = new Dictionary<ClassType, int>();
            foreach (var node in nodes)
            {
                var cls = getKnownClassifications(node);

                int count;
                result.TryGetValue(cls, out count);
                ++count;
                result[cls] = count;
            }

            return result;
        }

        private int getTotalCounts(Dictionary<ClassType, int> classCounts)
        {
            return classCounts.Values.Sum();
        }

        private ClassType getKnownClassifications(NodeReference node)
        {
            ClassType cls;
            _knownClassifications.TryGetValue(node, out cls);

            return cls;
        }

        #endregion
    }

    internal class KnowledgeRule
    {
        internal readonly IEnumerable<NodeReference> InitialYesNodes;

        internal readonly IEnumerable<NodeReference> InitialNoNodes;

        internal KnowledgeRule YesRule { get; private set; }

        internal KnowledgeRule NoRule { get; private set; }

        internal readonly IEnumerable<Tuple<string, bool>> Path;

        internal readonly NodeReference EndNode;

        internal KnowledgeRule(IEnumerable<Tuple<string, bool>> path, NodeReference endNode, IEnumerable<NodeReference> yesNodes, IEnumerable<NodeReference> noNodes)
        {
            InitialYesNodes = yesNodes.ToArray();
            InitialNoNodes = noNodes.ToArray();

            Path = path.ToArray();
            EndNode = endNode;
        }

        internal void AddNoBranch(KnowledgeRule knowledgeRule)
        {
            if (NoRule != null)
                throw new NotSupportedException("Cannot set rule branch twice");

            NoRule = knowledgeRule;
        }

        internal void AddYesBranch(KnowledgeRule knowledgeRule)
        {
            if (YesRule != null)
                throw new NotSupportedException("Cannot set rule branch twice");

            YesRule = knowledgeRule;
        }
    }
}
