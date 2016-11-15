using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

using KnowledgeDialog.Knowledge;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class EntityExtractor
    {
        internal readonly FreebaseDbProvider Db;

        private readonly Dictionary<string, int> _badNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _preBadNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _leadingNgramCounts = new Dictionary<string, int>();


        internal EntityExtractor(FreebaseDbProvider db)
        {
            Db = db;
        }

        private Dictionary<string, EntityInfo> rawScores(string[] ngrams, int aliasLength, double leadingScoreFactor)
        {
            int leadingScore = 0;
            var scores = new Dictionary<string, EntityInfo>();
            var skipNext = false;
            foreach (var ngram in ngrams)
            {
                int badCount;
                _badNgramCounts.TryGetValue(ngram, out badCount);
                if (badCount > 0)
                    continue;

                int preBadCount;
                _preBadNgramCounts.TryGetValue(ngram, out preBadCount);
                if (preBadCount > 10)
                {
                    skipNext = true;
                    continue;
                }

                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                var scoredDocs = Db.GetScoredContentDocs(ngram);
                foreach (var dc in scoredDocs)
                {
                    var mid = Db.GetMid(dc);
                    var content = Db.GetContent(dc);
                    var isAlias = content.Length < aliasLength;
                    var score = dc.Score;
                    score = score * ngram.Length;
                    score = score + leadingScore * (float)leadingScoreFactor;
                    if (content.ToLowerInvariant() == ngram.ToLowerInvariant())
                    {
                        score *= 5 * ngram.Length;
                    }
                    if (isAlias)
                    {
                        var lengthDiff = Math.Abs(content.Length - ngram.Length);
                        score = score / content.Length * 2;
                    }
                    else
                    {
                        score = score / 15;
                    }

                    EntityInfo entity;
                    if (!scores.TryGetValue(mid, out entity))
                    {
                        scores[mid] = entity = Db.CreateEntity(mid, dc);
                    }

                    score = entity.InBounds + entity.OutBounds;
                    scores[mid] = entity.AddScore(content, score);
                }

                int currentScore;
                _leadingNgramCounts.TryGetValue(ngram, out currentScore);
                leadingScore += currentScore;
            }
            /*
            var sum = scores.Values.Sum();
            foreach (var key in scores.Keys.ToArray())
            {
                scores[key] = scores[key] / sum;
            }
            */
            return scores;
        }

        internal Dictionary<string, EntityInfo> RawScores(string[] ngrams)
        {
            var aliasLength = 15;
            return rawScores(ngrams, aliasLength, 3);
        }

        internal IEnumerable<EntityInfo> GetEntities(string ngram)
        {
            var scores = new Dictionary<string, EntityInfo>();
            var scoredDocs = Db.GetScoredContentDocs(ngram);
            foreach (var dc in scoredDocs)
            {
                var mid = Db.GetMid(dc);
                var content = Db.GetContent(dc);
                var isAlias = content.Length < 15;
                var score = dc.Score;
                score = score * ngram.Length;
                if (content.ToLowerInvariant() == ngram.ToLowerInvariant())
                {
                    score *= 5 * ngram.Length;
                }
                if (isAlias)
                {
                    var lengthDiff = Math.Abs(content.Length - ngram.Length);
                    score = score / content.Length * 2;
                }
                else
                {
                    score = score / 15;
                }

                EntityInfo entity;
                if (!scores.TryGetValue(mid, out entity))
                {
                    scores[mid] = entity = Db.CreateEntity(mid, dc);
                }
                scores[mid] = entity.AddScore(content, score);
            }

            return scores.Values;
        }

        internal EntityInfo[] Score(string[] ngrams, string[] contextNgrams)
        {
            var scores = RawScores(ngrams);
            if (scores.Count == 0)
                return new EntityInfo[0];

            var badScores = rawScores(contextNgrams, 15, 3.0);
            foreach (var badScore in badScores)
            {
                //break;
                if (scores.ContainsKey(badScore.Key))
                    scores[badScore.Key] = scores[badScore.Key].SubtractScore(badScore.Value.Score / 10);
            }

            var rankedAnswers = new List<EntityInfo>();
            foreach (var pair in scores)
            {
                rankedAnswers.Add(pair.Value);
            }

            rankedAnswers.Sort();
            rankedAnswers.Reverse();
            return rankedAnswers.ToArray();
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
