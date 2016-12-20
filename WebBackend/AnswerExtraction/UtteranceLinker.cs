using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class UtteranceLinker
    {
        protected readonly FreebaseDbProvider Db;

        private readonly Dictionary<string, int> _badNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _preBadNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _leadingNgramCounts = new Dictionary<string, int>();


        private readonly HashSet<string> _nonInformativeWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "'s","a","an","the","I","we","he","she","it","you","they","me",
            "think","thought","know","knew","assume","assumed", "look","looked",
            "have","has","had","can","could",
            "is","are","were","was","been","would",
            "in","on","at","to","from","there","that",
            "who","why","what","where","which","whose","how",
            "with","and","or","any","neither", "out", "by","of",
            "up","down","top","bottom"
        };

        internal UtteranceLinker(FreebaseDbProvider db, string verbsLexicon = null)
        {
            Db = db;
            _nonInformativeWords.UnionWith(loadVerbs(verbsLexicon));
        }

        internal virtual IEnumerable<LinkedUtterance> LinkUtterance(string utterance, int entityHypCount)
        {
            var sanitizedUtterance = utterance.Replace(".", " ").Replace(",", " ").Replace("?", " ").Replace("!", " ").Replace("`s", "'s").Replace("'s", " 's");
            var index = new EntityIndex(sanitizedUtterance.Split(' ').Where(w => w.Length > 0).ToArray(), this, entityHypCount);
            var result = index.LinkedUtterance_Hungry();

            return new LinkedUtterance[] { result };
        }

        internal virtual IEnumerable<EntityInfo> GetValidEntities(string ngram, int entityHypothesisCount)
        {
            var words = ngram.Split(' ');
            var informativeWords = words.Where(w => !_nonInformativeWords.Contains(w)).ToArray();

            if (!informativeWords.Any())
                return new EntityInfo[0];

            var entities = GetEntities(ngram)
                .Where(e => e.Label != null)
                .Where(e => e.Description != null)
                .OrderByDescending(e => e.InBounds + e.OutBounds).ToArray();

            entities = pruneEntities(entities, entityHypothesisCount).ToArray();


            var nonInformativeWords = words.Except(informativeWords).Select(w => w.ToLowerInvariant()).ToList();

            foreach (var entity in entities)
            {
                var aliases = new[] { entity.Label }.Concat(Db.GetAliases(entity.Mid));
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

        protected virtual IEnumerable<EntityInfo> pruneEntities(IEnumerable<EntityInfo> entities, int entityHypothesisCount)
        {
            return entities.OrderByDescending(e => e.Score).Take(entityHypothesisCount);
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

       
        internal IEnumerable<EntityInfo> GetEntities(string ngram)
        {
            var scores = new Dictionary<string, EntityInfo>();
            var scoredDocs = Db.GetScoredContentDocs(ngram);
            foreach (var dc in scoredDocs)
            {
                var mid = Db.GetMid(dc);
                var content = Db.GetContent(dc);
                var category = Db.GetContentCategory(dc);
                var isLabel = category == ContentCategory.L;
                var isAlias = category == ContentCategory.A || isLabel;

                var score = dc.Score;
                score = score * ngram.Length;

                if (isLabel)
                {
                    score *= 2;
                }

                if (content.ToLowerInvariant() == ngram.ToLowerInvariant())
                {
                    //exact match
                    score *= 5 * ngram.Length;
                }

                if (isAlias)
                {
                    var lengthDiff = Math.Abs(content.Length - ngram.Length);
                    if (lengthDiff > 3)
                        //difference is too large
                        continue;
                    score = score / content.Length * 2;
                }
                else
                {
                    score = score / 15;
                }

                EntityInfo entity;
                if (!scores.TryGetValue(mid, out entity))
                {
                    scores[mid] = entity = Db.GetEntityInfoFromMid(mid);
                }
                scores[mid] = entity.AddScore(content, score);
            }

            return scores.Values;
        }
                
        internal void Train(IEnumerable<string> ngrams, string correctAnswer)
        {
            string strongestNgram = null;
            string preBadNgram = null;
            var bestNgramScore = 0.0;

            foreach (var ngram in ngrams)
            {
                var docs = Db.GetScoredContentDocs(ngram);
                var bestDoc = docs.FirstOrDefault();
                if (bestDoc == null)
                    //nothing found for the ngram
                    continue;

                var id = Db.GetMid(bestDoc);
                if (id == correctAnswer)
                {
                    //the ngram is helpful
                    if (bestDoc.Score > bestNgramScore)
                    {
                        strongestNgram = ngram;
                        bestNgramScore = bestDoc.Score;
                    }
                }
                else
                {
                    int count;
                    _badNgramCounts.TryGetValue(ngram, out count);
                    _badNgramCounts[ngram] = count + 1;

                    if (preBadNgram != null)
                    {
                        _preBadNgramCounts.TryGetValue(preBadNgram, out count);
                        _preBadNgramCounts[preBadNgram] = count + 1;
                    }

                }
                preBadNgram = ngram;
            }

            var orderedNgrams = ngrams.ToArray();
            for (var i = 1; i < orderedNgrams.Length; ++i)
            {
                if (orderedNgrams[i] == strongestNgram)
                {
                    var preNgram = orderedNgrams[i - 1];
                    int value;
                    _leadingNgramCounts.TryGetValue(preNgram, out value);
                    _leadingNgramCounts[preNgram] = value + 1;
                    break;
                }
            }
        }
    }
}
