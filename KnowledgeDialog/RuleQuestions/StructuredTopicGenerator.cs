using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.RuleQuestions
{
    class StructuredTopicGenerator
    {
        public readonly ComposedGraph Graph;

        public bool IsEnd { get; private set; }

        public IEnumerable<KnowledgeConstraint> TopicConstraints { get { return _currentTopicConstraints; } }

        private readonly KnowledgeConstraintOptions[] _constraintOptions;

        private KnowledgeConstraint[] _currentTopicConstraints;

        private int[] _currentIndexes;


        public StructuredTopicGenerator(IEnumerable<KnowledgeConstraintOptions> constraintOptions, ComposedGraph graph)
        {
            Graph = graph;
            _constraintOptions = constraintOptions.ToArray();
        }

        internal bool MoveNext()
        {
            if (_currentIndexes == null)
            {
                _currentIndexes = new int[_constraintOptions.Length];
            }
            else if (!shiftNext(_currentIndexes, _constraintOptions))
            {
                IsEnd = true;
                return false;
            }

            _currentTopicConstraints = createCurrentConstraints().ToArray();
            return true;
        }

        private IEnumerable<KnowledgeConstraint> createCurrentConstraints()
        {
            var currentConstraints = new List<KnowledgeConstraint>();
            for (var i = 0; i < _currentIndexes.Length; ++i)
            {
                var currentIndex = _currentIndexes[i];
                var currentConstraint = _constraintOptions[i].GetConstraint(currentIndex);
                currentConstraints.Add(currentConstraint);
            }

            return currentConstraints;
        }

        internal ConstraintSelector InitializeSelector(IEnumerable<NodeReference> constraintsMapping, NodeReference answer)
        {
            HashSet<NodeReference> selectedNodes = null;
            var constraints = createCurrentConstraints().ToArray();
            foreach (var constraint in constraintsMapping.Zip(constraints, Tuple.Create))
            {
                var constraintSet = constraint.Item2.FindSet(constraint.Item1, Graph);
                if (selectedNodes == null)
                {
                    selectedNodes = new HashSet<NodeReference>(constraintSet);
                }
                else
                {
                    selectedNodes.IntersectWith(constraintSet);
                }
            }

            if (selectedNodes == null || selectedNodes.Count == 0)
                return null;

            return new ConstraintSelector(Graph, answer, selectedNodes);
        }

        private HashSet<NodeReference> findSet(NodeReference constraintNode, KnowledgeConstraint constraint)
        {
            return constraint.FindSet(constraintNode, Graph);
        }

        private bool shiftNext(int[] indexes, KnowledgeConstraintOptions[] constraintOptions)
        {
            //this works like number adding
            for (var i = 0; i < indexes.Length; ++i)
            {
                indexes[i] += 1;
                if (indexes[i] < constraintOptions[i].Count)
                    //no "carry" to next digit
                    return true;

                indexes[i] = 0;
            }
            return false;
        }
    }
}
