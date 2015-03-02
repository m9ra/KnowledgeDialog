using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    class PathSegment
    {
        public readonly PathSegment PreviousSegment;

        public readonly NodeReference Node;

        public readonly string Edge;

        public readonly bool IsOutcoming;

        public PathSegment(PathSegment previousSegment, string edge,bool isOutcoming, NodeReference toNode)
        {
            PreviousSegment = previousSegment;
            Edge = edge;
            Node = toNode;
            IsOutcoming = isOutcoming;
        }
    }
}
