using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    /// <summary>
    /// Reference to node in a graph.
    /// </summary>
    public class NodeReference
    {
        /// <summary>
        /// Data represented by node.
        /// </summary>
        public readonly string Data;

        internal NodeReference(string data)
        {
            Data = data;
        }

        public override string ToString()
        {
            return "[Ref: " + Data.ToString() + "]";
        }

        public override bool Equals(object obj)
        {
            var o = obj as NodeReference;
            if (o == null)
                return false;

            return Data.Equals(o.Data, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Data.ToLower().GetHashCode();
        }
    }
}
