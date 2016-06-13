using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.AnswerExtraction
{
    class UtteranceLinker
    {
        private readonly EntityExtractor _extractor;

        private readonly HashSet<string> _nonInformativeWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "'s","a","an","the","I","we","he","she","it","you","they","me",
            "think","thought","know","knew","assume","assumed", "look","looked",
            "have","has","had","can","could",
            "is","are","were","was","been","would",
            "in","on","to","from","there","that",
            "who","why","what","where","which","whose","how",
            "with","and","or","any","neither", "out", "by","of",
            "up","down","top","bottom"
        };

        internal UtteranceLinker(EntityExtractor extractor, string verbsLexicon = null)
        {
            _extractor = extractor;
            _nonInformativeWords.UnionWith(loadVerbs(verbsLexicon));
        }

        internal IEnumerable<LinkedUtterance> LinkUtterance(string utterance, int entityHypCount)
        {
            var sanitizedUtterance = utterance.Replace(".", " ").Replace(",", " ").Replace("?", " ").Replace("!", " ").Replace("`s", "'s").Replace("'s", " 's");
            var index = new EntityIndex(sanitizedUtterance.Split(' ').Where(w => w.Length > 0).ToArray(), this, entityHypCount);
            var result = index.LinkedUtterance_Hungry();

            return new LinkedUtterance[] { result };
        }

        internal IEnumerable<EntityInfo> GetValidEntities(string ngram, int entityHypothesisCount)
        {
            var words = ngram.Split(' ');
            var informativeWords = words.Where(w => !_nonInformativeWords.Contains(w)).ToArray();

            if (!informativeWords.Any())
                return new EntityInfo[0];

            var entities = _extractor.GetEntities(ngram)
                .Where(e => e.Label != null)
                .Where(e => _extractor.GetDescription(e.Mid) != null)
                .OrderByDescending(e => e.InBounds + e.OutBounds).ToArray();

            entities = disambiguateTo(entities, entityHypothesisCount).ToArray();


            var nonInformativeWords = words.Except(informativeWords).Select(w => w.ToLowerInvariant()).ToList();

            foreach (var entity in entities)
            {
                var aliases = new[] { entity.Label }.Concat(_extractor.GetAliases(entity.Mid));
                foreach (var alias in aliases)
                {
                    var entityWords = alias.ToLowerInvariant().Split(' ');
                    foreach (var word in entityWords)
                        nonInformativeWords.Remove(word);
                }
            }

            if (nonInformativeWords.Count > 0)
                return new EntityInfo[0];

            return entities;
        }

        protected virtual IEnumerable<EntityInfo> disambiguateTo(IEnumerable<EntityInfo> entities, int entityHypothesisCount)
        {
            return entities.Take(entityHypothesisCount);
        }

        private IEnumerable<string> loadVerbs(string path)
        {
            if (path == null)
                yield break;

            var lines = File.ReadLines(path);
            foreach (var line in lines)
            {
                var verbForms = line.Split(' ');
                var basicForm = verbForms[0];

                yield return basicForm;
                if (verbForms.Length == 1)
                {
                    //regular verb
                    if (basicForm.EndsWith("e"))
                        yield return basicForm + "d";
                    else
                        yield return basicForm + "ed";
                }
                else
                {
                    //irregular verb
                    yield return verbForms[1];
                    yield return verbForms[2];
                }
            }
        }
    }
}
