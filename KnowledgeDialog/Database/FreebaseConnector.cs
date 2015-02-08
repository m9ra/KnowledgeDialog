using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.Database
{
    class FreebaseConnector : GraphLayerBase
    {
        protected internal override IEnumerable<string> Edges(NodeReference from, NodeReference to)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<KeyValuePair<string, NodeReference>> Incoming(NodeReference node)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<KeyValuePair<string, NodeReference>> Outcoming(NodeReference node)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<NodeReference> Outcoming(NodeReference fromNode, string edge)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<NodeReference> Incoming(NodeReference toNode, string edge)
        {
            throw new NotImplementedException();
        }

        internal override GraphLayerBase Snapshot()
        {
            throw new NotImplementedException();
        }
    }
}
