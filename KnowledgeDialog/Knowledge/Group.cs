using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    public class Group
    {
        /// <summary>
        /// Graph which this pattern is mapped to.
        /// </summary>
        private readonly ComposedGraph _graph;

        /// <summary>
        /// Nodes that are contained by the pattern.
        /// </summary>
        private readonly HashSet<NodeReference> _groupNodes = new HashSet<NodeReference>();

        /// <summary>
        /// How many nodes are stored within the group.
        /// </summary>
        public int Count { get { return _groupNodes.Count; } }

        public Group(ComposedGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Adds node to the pattern.
        /// </summary>
        /// <param name="node">The node to be added.</param>
        public void AddNode(NodeReference node)
        {
            _groupNodes.Add(node);
        }

        /// <summary>
        /// Finds clustering of group nodes according to common edges.
        /// (Pattern is clustered by edges (not nodes) only).
        /// 
        /// The pattern nodes are always present as path starting nodes.
        /// </summary>
        /// <returns>The node clustering.</returns>
        public MultiTraceLog2 FindEdgePattern(int maxLength, int maxWidth)
        {
            var pattern=new MultiTraceLog2(_groupNodes, _graph, false, maxWidth, maxLength);
            if(!pattern.TraceNodes.Skip(1).Any())
                pattern = new MultiTraceLog2(_groupNodes, _graph, true, maxWidth, 1);

            return pattern;
        }
    }
}
