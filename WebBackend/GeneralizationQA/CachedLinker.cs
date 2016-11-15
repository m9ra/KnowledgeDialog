using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Parsing;

using WebBackend.AnswerExtraction;

namespace WebBackend.GeneralizationQA
{
    class CachedLinker
    {
        /// <summary>
        /// Utterances which linked form was cached.
        /// </summary>
        private readonly Dictionary<string, LinkedUtterance> _cachedUtterances = new Dictionary<string, LinkedUtterance>();

        private readonly GraphDisambiguatedLinker _linker;

        internal CachedLinker(string[] utterances, LinkedUtterance[] linkedUtterances, GraphDisambiguatedLinker linker)
        {
            _linker = linker;

            for (var i = 0; i < utterances.Length; ++i)
            {
                var utterance = utterances[i];
                var linkedUtterance = linkedUtterances[i];

                _cachedUtterances[utterance] = linkedUtterance;
            }
        }

        public LinkedUtterance LinkUtterance(string utterance)
        {
            LinkedUtterance result;
            if (!_cachedUtterances.TryGetValue(utterance, out result))
                _cachedUtterances[utterance] = result = _linker.LinkUtterance(utterance, 1).First();

            return result;
        }
    }
}
