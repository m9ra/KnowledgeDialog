using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuestions
{
    class FreeBaseEdge
    {
        static internal readonly string FreebaseEdgePrefix = "www.freebase.com/";

        internal string EdgeId;

        internal FreeBaseEdge(string edgeRepresentation)
        {
            if (!edgeRepresentation.StartsWith(FreebaseEdgePrefix))
                throw new NotSupportedException("edge format");

            EdgeId = edgeRepresentation.Substring(FreebaseEdgePrefix.Length);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return EdgeId.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var o = obj as FreeBaseEdge;
            if (o == null)
                return false;

            return EdgeId.Equals(o.EdgeId);
        }
    }
}
