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


        private readonly HashSet<string> _nonInformativeWords1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "'s","a","an","the","I","we","he","she","it","you","they","me",
            "Im","its","his","her","their","us","our",
            "think","thought","know","knew","assume","assumed", "look","looked",
            "have","has","had","can","could",
            "is","are","were","was","been","would",
            "in","on","at","to","from","there","that",
            "who","why","what","where","which","whose","how", "when","whenever","whatever","anywhere",
            "much","many",
            "with","without","within",
            "and","or","any","neither", "out", "by","of",
            "up","down","top","bottom",
            "only", "for", "believe", "so",
            "do","did","done","does",
            "wont","will","would",
            "have","has","had",
            "all","every","each","never","ever","always",
        };

        private readonly HashSet<string> _nonInformativeWords2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal UtteranceLinker(FreebaseDbProvider db, string verbsLexicon = null)
        {
            Db = db;
            _nonInformativeWords2.UnionWith(_nonInformativeWords1);
            _nonInformativeWords2.UnionWith(loadVerbs(verbsLexicon));
        }

        internal virtual IEnumerable<LinkedUtterance> LinkUtterance(string utterance, int entityHypCount)
        {
            if (utterance.Length > 200)
                //the utterance is too long
                return Enumerable.Empty<LinkedUtterance>();

            var sanitizedUtterance = utterance.ToLowerInvariant().Replace(".", " ").Replace(",", " ").Replace("(", " ").Replace(")", " ").Replace("?", " ").Replace("!", " ").Replace("`s", "'s").Replace("'s", " 's").Replace("s'", " s'");
            var index = new EntityIndex(sanitizedUtterance.Split(' ').Where(w => w.Length > 0).ToArray(), this, entityHypCount);
            var result = index.LinkedUtterance_Hungry();

            return new LinkedUtterance[] { result };
        }

        internal virtual IEnumerable<EntityInfo> GetValidEntities(string ngram, int entityHypothesisCount)
        {
            var words = ngram.Split(' ');
            var informativeWords = words.Where(w => !_nonInformativeWords2.Contains(w)).ToArray();

            if (!informativeWords.Any())
                return new EntityInfo[0];

            var entities = GetEntities(ngram).ToArray();
            entities = entities
                .Where(e => e.Label != null)
                .Where(e => e.Description != null)
                .Where(e => KnowledgeDialog.Dialog.Parsing.Utilities.Levenshtein(e.BestAliasMatch.ToLowerInvariant(), ngram) < 3)
                .OrderByDescending(e => e.InBounds + e.OutBounds).ToArray();

            var rescoredEntities = new List<EntityInfo>();
            foreach (var entity in entities)
            {
                var lengthFactor = (ngram.Length + entity.BestAliasMatch.Length) / 2.0;
                var distance = KnowledgeDialog.Dialog.Parsing.Utilities.Levenshtein(entity.BestAliasMatch.ToLowerInvariant(), ngram);
                var matchScore = lengthFactor / (lengthFactor + distance);
                var newEntityScore = lengthFactor * matchScore;

                var newEntity = entity.WithScore(newEntityScore);
                rescoredEntities.Add(newEntity);
            }

            entities = rescoredEntities.OrderByDescending(e => e.Score).ToArray();
            entities = pruneEntities(entities, entityHypothesisCount).ToArray();


            var nonInformativeWords = words.Except(informativeWords).Select(w => w.ToLowerInvariant()).ToList();
            var canBeUsed = nonInformativeWords.Count == 0;
            foreach (var entity in entities)
            {
                if (canBeUsed)
                    break;

                var aliases = new[] { entity.Label }.Concat(Db.GetAliases(entity.Mid));
                foreach (var alias in aliases)
                {
                    var entityWords = alias.ToLowerInvariant().Split(' ');
                    if (nonInformativeWords.Intersect(entityWords).Count() == nonInformativeWords.Count)
                    {
                        canBeUsed = true;
                        break;
                    }
                }
            }

            if (!canBeUsed)
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
                    yield return basicForm + "s";
                    yield return basicForm + "ing";
                    if (basicForm.EndsWith("e"))
                    {
                        yield return basicForm + "d";
                    }
                    else
                    {
                        yield return basicForm + "ed";
                    }
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
            var scoredDocs = Db.GetScoredDocs(ngram);
            foreach (var dc in scoredDocs)
            {
                var entity = Db.GetEntity(dc);
                var score = dc.Score;
                score = score * ngram.Length;

                /* if (isLabel)
                 {
                     score *= 2;
                 }*/

                /*if (content.ToLowerInvariant() == ngram.ToLowerInvariant())
                {
                    //exact match
                    score *= 5 * ngram.Length;
                }*/

                var mid = entity.Mid;
                EntityInfo storedEntity;
                if (!scores.TryGetValue(mid, out storedEntity))
                {
                    scores[mid] = storedEntity = entity;
                }
                scores[mid] = storedEntity.AddScore(ngram, score);
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
                var docs = Db.GetScoredDocs(ngram);
                if (!docs.Any())
                    //nothing found for the ngram
                    continue;

                var bestDoc = docs.First();
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
