using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;
using KnowledgeDialog.Knowledge;

using WebBackend.Dataset;

namespace WebBackend.GeneralizationQA
{
    delegate LinkedUtterance Linker(string utterance);

    class PatternGeneralizer
    {
        private readonly ComposedGraph _graph;

        private readonly Linker _linker;

        private readonly Dictionary<string, Group> _answerGroups = new Dictionary<string, Group>();

        private readonly Dictionary<MultiTraceLog, TraceNode[]> _cachedFilteredTraces = new Dictionary<MultiTraceLog, TraceNode[]>();

        public PatternGeneralizer(ComposedGraph graph, Linker linker)
        {
            _graph = graph;
            _linker = linker;
        }

        public void AddExample(string question, NodeReference answer)
        {
            string questionSignature;
            IEnumerable<NodeReference> questionEntities;

            extractSignature(question, out questionSignature, out questionEntities);
            var answerGroup = GetAnswerGroup(questionSignature);
            answerGroup.AddNode(answer);
            //TODO the language statistics should be counted here
        }

        public Ranked<NodeReference> GetAnswer(string question)
        {
            string questionSignature;
            IEnumerable<NodeReference> questionEntities;

            extractSignature(question, out questionSignature, out questionEntities);

            var rankedCandidates = new List<Ranked<NodeReference>>();
            var signatures = findMatchingSignatures(questionSignature).OrderByDescending(s => s.Rank).ToArray();
            foreach (var rankedSignature in signatures)
            {
                /*if (rankedSignature.Value != questionSignature)
                    continue;*/

                if (rankedSignature.Rank <= 0.5)
                    continue;

                var answerGroup = GetAnswerGroup(rankedSignature.Value);

                var pattern = answerGroup.FindEdgePattern(1, 1);
                var match = PatternMatchProbability(pattern, questionSignature, questionEntities, _graph);
                if (match == null)
                    //pattern does not match
                    continue;

                //find answer candidates (intersect substitution paths)
                var answerGroupCandidates = findCandidates(match);
                //rank them according to answer pattern match
                var rankedGroupCandidates = rankCandidates(answerGroupCandidates, pattern);
                var orderedGroupCandidates = rankedGroupCandidates.OrderByDescending(c => c.Rank).ToArray();
                var bestGroupCandidate = orderedGroupCandidates.FirstOrDefault();
                if (bestGroupCandidate == null)
                    continue;

                bestGroupCandidate = new Ranked<NodeReference>(bestGroupCandidate.Value, bestGroupCandidate.Rank * rankedSignature.Rank);
                rankedCandidates.Add(bestGroupCandidate);
            }

            var orderedCandidates = rankedCandidates.OrderByDescending(r => r.Rank).ToArray();
            var bestCandidate = orderedCandidates.FirstOrDefault();
            return bestCandidate;
        }

        private IEnumerable<Ranked<string>> findMatchingSignatures(string signature)
        {
            var words = getWordsForSignatureComparison(signature);

            var result = new List<Ranked<string>>();
            foreach (var knownSignature in _answerGroups.Keys)
            {
                if (knownSignature == signature)
                {
                    // perfect match
                    result.Add(new Ranked<string>(knownSignature, 1.0));
                    continue;
                }

                var knownSignatureWords = getWordsForSignatureComparison(knownSignature);
                var commonWords = words.Intersect(knownSignatureWords).ToArray();

                var similarity = 2.0 * commonWords.Length / (words.Length + knownSignatureWords.Length);
                result.Add(new Ranked<string>(knownSignature, similarity));
            }

            return result;
        }

        private string[] getWordsForSignatureComparison(string signature)
        {
            return signature.ToLowerInvariant().Split(' ').Where(w => !w.StartsWith("$")).ToArray();
        }

        private IEnumerable<Ranked<NodeReference>> rankCandidates(IEnumerable<NodeReference> answerCandidates, MultiTraceLog pattern)
        {
            var candidates = new List<Ranked<NodeReference>>();
            foreach (var answerCandidate in answerCandidates)
            {
                var rank = getRank(answerCandidate, pattern);
                candidates.Add(new Ranked<NodeReference>(answerCandidate, rank));
            }
            return candidates;
        }

        private double getRank(NodeReference answerCandidate, MultiTraceLog pattern)
        {
            var commonTraceNodes = getCommonTraceNodes(answerCandidate, pattern);
            var rank = 0.0;
            foreach (var traceNode in commonTraceNodes)
            {
                var initialNodes = traceNode.Traces.Select(t => t.InitialNodes).Distinct().ToArray();
                var edgeImportance = 1.0 * initialNodes.Length / pattern.NodeBatch.Count();
                rank += edgeImportance;
            }

            //TODO here could be more precise formula
            return rank / (pattern.TraceNodes.Count() - 1);
        }

        private IEnumerable<TraceNode> getCommonTraceNodes(NodeReference answerCandidate, MultiTraceLog pattern)
        {
            var compatibleTraces = new List<TraceNode>();
            var filteredTraces = getFilteredTraces(pattern);
            foreach (var node in filteredTraces)
            {
                if (node.PreviousNode == null)
                    //we are not interested in root
                    continue;

                var tracePath = node.GetPathFromRoot();
                if (_graph.GetForwardTargets(new[] { answerCandidate }, tracePath).Any())
                    compatibleTraces.Add(node);
            }

            return compatibleTraces;
        }

        private IEnumerable<NodeReference> findCandidates(PatternSubstitutionMatch match)
        {
            var targets = new HashSet<NodeReference>();
            var containsEverything = true;
            foreach (var substitution in match.SubstitutionPaths)
            {
                var substitutionTargets = substitution.FindTargets(_graph);
                if (containsEverything)
                {
                    //before first constraint is found we pretend targets contains
                    //every node
                    containsEverything = false;
                    targets.UnionWith(substitutionTargets);
                }
                else
                {
                    targets.IntersectWith(substitutionTargets);
                }
            }

            return targets;
        }

        private void extractSignature(string question, out string questionSignature, out IEnumerable<NodeReference> questionEntities)
        {
            var linkedQuestion = _linker(question);
            var entities = new List<NodeReference>();

            var signature = new StringBuilder();
            foreach (var part in linkedQuestion.Parts)
            {
                var partEntities = part.Entities.Select(e => _graph.GetNode(FreebaseLoader.GetId(e.Mid))).ToArray();
                entities.AddRange(partEntities);

                if (signature.Length > 0)
                    signature.Append(' ');

                if (partEntities.Length == 0)
                    signature.Append(part.Token);
                else
                    signature.Append("$" + entities.Count);
            }

            questionEntities = entities;
            questionSignature = signature.ToString();
        }

        internal PatternSubstitutionMatch PatternMatchProbability(MultiTraceLog pattern, string questionSignature, IEnumerable<NodeReference> questionEnities, ComposedGraph graph)
        {
            var bestSubstitutions = new List<PathSubstitution>();
            foreach (var node in questionEnities)
            {
                var currentBestConfidence = 0.0;
                PathSubstitution currentBestPath = null;
                foreach (var path in getSubstitutedPaths(pattern, node, graph))
                {
                    var currentPath = path;
                    var originalNodes = currentPath.CompatibleInitialNodes;
                    var confidence = currentPath.Rank / pattern.NodeBatch.Count();
                    //TODO consider context 
                    confidence *= SubstitutionProbability(currentPath.Substitution, originalNodes);
                    currentPath = currentPath.Reranked(confidence);
                    if (currentBestConfidence < confidence)
                    {
                        currentBestPath = path;
                        currentBestConfidence = confidence;
                    }
                }

                if (currentBestPath == null)
                    //substitution was not found
                    return null;

                bestSubstitutions.Add(currentBestPath);
            }


            //TODO we would like to have match information in the output
            return new PatternSubstitutionMatch(bestSubstitutions);
        }

        internal double SubstitutionProbability(NodeReference substitution, IEnumerable<NodeReference> originalNodes)
        {
            return 1.0; //TODO consider substitution quality
        }

        private IEnumerable<PathSubstitution> getSubstitutedPaths(MultiTraceLog pattern, NodeReference substitutionNode, ComposedGraph graph)
        {
            var substitution = new SubstitutionValidator(substitutionNode, graph);

            var filteredNodes = getFilteredTraces(pattern);
            foreach (var traceNode in filteredNodes.Reverse())
            {
                if (traceNode.CurrentEdge == null)
                    //we skip substitution to answer
                    continue;

                if (substitution.IsCompatible(traceNode))
                    yield return new PathSubstitution(substitutionNode, traceNode);
            }
        }

        private IEnumerable<TraceNode> getFilteredTraces(MultiTraceLog pattern)
        {
            TraceNode[] result;
            if (!_cachedFilteredTraces.TryGetValue(pattern, out result))
            {
                result = pattern.TraceNodes.Where(n =>
                {
                    //TODO this is workaround how to filter out irrelevant paths
                    var pathLen = n.Path.Count();
                    var distinctPathLen = n.Path.Distinct().Count();

                    return pathLen == distinctPathLen;
                }).OrderByDescending(t => t.CompatibleInitialNodes.Count()).Take(100).ToArray();

                _cachedFilteredTraces[pattern] = result;
            }

            return result;
        }

        internal Group GetAnswerGroup(string signature)
        {
            Group result;
            if (!_answerGroups.TryGetValue(signature, out result))
                _answerGroups[signature] = result = new Group(_graph);

            return result;
        }
    }
}
