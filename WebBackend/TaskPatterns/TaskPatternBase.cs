using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.TaskPatterns
{
    abstract class TaskPatternBase
    {
        public string PatternFormat { get; private set; }

        public int SubstitutionCount { get { return _substitutions.Count; } }

        private readonly List<NodeReference> _substitutions = new List<NodeReference>();

        private readonly ComposedGraph _graph;

        private readonly List<Tuple<string, bool>> _rules = new List<Tuple<string, bool>>();

        protected TaskPatternBase(ComposedGraph graph)
        {
            _graph = graph;
        }

        protected void SetPattern(string patternFormat)
        {
            if (PatternFormat != null)
                throw new NotSupportedException("Cannot set pattern format twice");

            PatternFormat = patternFormat;
        }

        protected void Substitutions(params string[] nodesData)
        {
            foreach (var nodeData in nodesData)
            {
                var node = _graph.GetNode(nodeData);
                _substitutions.Add(node);
            }
        }

        protected void ExpectedAnswerRule(IEnumerable<Tuple<string,bool>> ruleEdges)
        {
            _rules.AddRange(ruleEdges);
        }

        internal NodeReference GetSubstitution(int substitutionIndex)
        {
            return _substitutions[substitutionIndex];
        }

        internal IEnumerable<NodeReference> GetExpectedAnswers(int substitutionIndex)
        {
            return _graph.GetForwardTargets(new[] { GetSubstitution(substitutionIndex) }, _rules);
        }
    }
}
