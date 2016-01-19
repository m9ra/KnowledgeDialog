using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuestions
{
    class FreeBaseNode
    {
        static internal readonly string FreebaseNodePrefix = "www.freebase.com/m/";

        internal string NodeId;

        internal FreeBaseNode(string nodeRepresentation)
        {
            if (!nodeRepresentation.StartsWith(FreebaseNodePrefix))
                throw new NotSupportedException("node format");

            NodeId = nodeRepresentation.Substring(FreebaseNodePrefix.Length);

        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return NodeId.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as FreeBaseNode;
            if (o == null)
                return false;

            return NodeId.Equals(o.NodeId);
        }
    }
}
