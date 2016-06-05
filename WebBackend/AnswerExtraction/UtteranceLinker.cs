using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.AnswerExtraction
{
    class UtteranceLinker
    {
        private readonly Extractor _extractor;

        internal UtteranceLinker(Extractor extractor)
        {
            _extractor = extractor;
        }

        internal IEnumerable<LinkedUtterance> LinkUtterance(string utterance)
        {
            var sanitizedUtterance = utterance.Replace(".", " ").Replace(",", " ").Replace("?", " ").Replace("!", " ");
            var index = new EntityIndex(sanitizedUtterance.Split(' ').Where(w => w.Length > 0).ToArray(), this);
            var result = index.LinkedUtterance_Hungry();

            return new LinkedUtterance[] { result };
        }

        internal IEnumerable<EntityInfo> GetValidEntities(string ngram)
        {
            var entities = _extractor.GetEntities(ngram);

            var result = new List<EntityInfo>();
            foreach (var entity in entities)
            {
                var lengthDiff = ngram.Length - entity.BestAliasMatch.Length;
                if (entity.Label == null)
                    continue;

                var distance = UtteranceParser.Levenshtein(entity.Label, ngram);
                if (distance > 2)
                    //the difference is too large
                    continue;

                result.Add(entity);
            }

            return result;
        }
    }
}
