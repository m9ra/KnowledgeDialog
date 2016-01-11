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
    }
}
