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
    }
}
