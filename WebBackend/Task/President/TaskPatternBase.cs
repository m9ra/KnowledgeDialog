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

        private readonly List<IEnumerable<NodeReference>> _correctAnswers = new List<IEnumerable<NodeReference>>();

        private readonly ComposedGraph _graph;

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

        internal void AddTaskSubstitution(string substitution, IEnumerable<string> correctAnswers)
        {
            if (!_graph.HasEvidence(substitution))
                throw new NotSupportedException("Cannot create task with unknown substitution node " + substitution);

            if (!correctAnswers.Any())
                throw new NotSupportedException("Cannot create task with no answer nodes for substitution node " + substitution);

            foreach (var answer in correctAnswers)
                if (!_graph.HasEvidence(answer))
                    throw new NotSupportedException("Cannot create task with unknown answer node " + answer);

            var substitutionNode = _graph.GetNode(substitution);
            var correctAnswerNodes = from answer in correctAnswers select _graph.GetNode(answer);

            _substitutions.Add(substitutionNode);
            _correctAnswers.Add(correctAnswerNodes);
        }

        internal NodeReference GetSubstitution(int substitutionIndex)
        {
            return _substitutions[substitutionIndex];
        }

        internal IEnumerable<NodeReference> GetExpectedAnswers(int substitutionIndex)
        {
            return _correctAnswers[substitutionIndex];
        }
    }
}
