using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    class NodeRestriction
    {
        private readonly List<NodeRestriction> _otherRestrictions = new List<NodeRestriction>();

        private readonly List<string> _restrictionEdges = new List<string>();

        private readonly List<bool> _restrictionOutDirection = new List<bool>();

        private readonly HashSet<Tuple<NodeRestriction, string, bool>> _restrictions = new HashSet<Tuple<NodeRestriction, string, bool>>();

        public readonly NodeReference BaseNode;

        public int TargetsCount { get { return _restrictionEdges.Count; } }


        public NodeRestriction(NodeReference baseNode)
        {
            BaseNode = baseNode;
        }

        internal void AddEdge(string edge, bool outDirection, NodeRestriction restrictionTarget)
        {
            if (restrictionTarget == null)
                throw new ArgumentNullException("restrictionTarget");

            var restriction = Tuple.Create(restrictionTarget, edge, outDirection);
            if (!_restrictions.Add(restriction))
                //there is nothing to do
                return;

            _restrictionEdges.Add(edge);
            _restrictionOutDirection.Add(outDirection);
            _otherRestrictions.Add(restrictionTarget);
        }

        internal NodeRestriction GetTarget(int i)
        {
            return _otherRestrictions[i];
        }

        internal NodeRestriction GetTarget(KeyValuePair<string, bool> edge)
        {
            //TODO it may be faster by hashing
            for (var i = 0; i < _restrictionEdges.Count; ++i)
            {
                var otherEdge = _restrictionEdges[i];
                var otherIsOut = _restrictionOutDirection[i];
                if (edge.Key == otherEdge && edge.Value == otherIsOut)
                    return _otherRestrictions[i];
            }

            return null;
        }

        internal string GetEdge(int i)
        {
            return _restrictionEdges[i];
        }

        internal bool IsOutDirection(int i)
        {
            return _restrictionOutDirection[i];
        }

        public override string ToString()
        {
            return "#" + BaseNode.ToString();
        }
    }
}
