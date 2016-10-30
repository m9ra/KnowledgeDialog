using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.GeneralizationQA
{
    class SingleWordLinker
    {
        /// <summary>
        /// Nodes which are indexed (by their data)
        /// </summary>
        private readonly Dictionary<string, NodeReference> _indexedNodes = new Dictionary<string, NodeReference>();

        internal LinkedUtterance LinkUtterance(string utterance)
        {
            var parts = new List<LinkedUtterancePart>();
            var words = utterance.Split(' ');
            foreach (var word in words)
            {
                if (_indexedNodes.ContainsKey(word))
                {
                    var entityInfo = new EntityInfo(word, word, 1, 1);
                    parts.Add(LinkedUtterancePart.Entity(word, new[] { entityInfo }));
                }
                else
                {
                    parts.Add(LinkedUtterancePart.Word(word));
                }
            }

            return new LinkedUtterance(parts);
        }

        internal void Add(params NodeReference[] indexedNodes)
        {
            foreach (var node in indexedNodes)
            {
                _indexedNodes.Add(node.Data, node);
            }
        }
    }
}
