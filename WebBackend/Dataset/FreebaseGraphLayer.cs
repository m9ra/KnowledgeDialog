using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Dataset
{
    class FreebaseGraphLayer : GraphLayerBase
    {
        private readonly FreebaseDbProvider _db;

        internal FreebaseGraphLayer(FreebaseDbProvider db)
        {
            _db = db;
        }

        /// </inheritdoc>
        protected override IEnumerable<string> Edges(NodeReference from, NodeReference to)
        {
            throw new NotImplementedException();
        }

        /// </inheritdoc>
        protected override IEnumerable<KeyValuePair<string, NodeReference>> Incoming(NodeReference node)
        {
            var id = node.Data;
            var entity = _db.GetEntryFromId(id);

            foreach (var target in entity.Targets)
            {
                if (target.Item1.IsOutcoming)
                    continue;

                yield return new KeyValuePair<string, NodeReference>(target.Item1.Name, CreateReference(target.Item2));
            }
        }

        /// </inheritdoc>
        protected override IEnumerable<KeyValuePair<string, NodeReference>> Outcoming(NodeReference node)
        {
            var id = node.Data;
            var entity = _db.GetEntryFromId(id);

            foreach (var target in entity.Targets)
            {
                if (!target.Item1.IsOutcoming)
                    continue;

                yield return new KeyValuePair<string, NodeReference>(target.Item1.Name, CreateReference(target.Item2));
            }
        }

        /// </inheritdoc>
        protected override IEnumerable<NodeReference> Outcoming(NodeReference fromNode, string edge)
        {
            var id = fromNode.Data;
            var entity = _db.GetEntryFromId(id);

            foreach (var target in entity.Targets)
            {
                if (!target.Item1.IsOutcoming)
                    continue;

                yield return CreateReference(target.Item2);
            }
        }

        /// </inheritdoc>
        protected override IEnumerable<NodeReference> Incoming(NodeReference toNode, string edge)
        {
            var id = toNode.Data;
            var entity = _db.GetEntryFromId(id);

            foreach (var target in entity.Targets)
            {
                if (target.Item1.IsOutcoming)
                    continue;

                yield return CreateReference(target.Item2);
            }
        }
    }
}
