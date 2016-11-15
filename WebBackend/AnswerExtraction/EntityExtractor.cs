using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Store = Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

using KnowledgeDialog.Knowledge;

namespace WebBackend.AnswerExtraction
{
    internal enum ContentCategory { L, A, D };

    class EntityExtractor
    {
        internal static readonly string IdPrefix = "www.freebase.com/m/";

        /// <summary>
        /// Path to index directory.
        /// </summary>
        private readonly string _indexPath;

        private readonly Analyzer _analyzer;

        private readonly Dictionary<string, ScoreDoc[]> _scoredDocsCache = new Dictionary<string, ScoreDoc[]>();

        private readonly Dictionary<string, int> _badNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _preBadNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _leadingNgramCounts = new Dictionary<string, int>();

        private readonly QueryParser _contentParser;

        private readonly QueryParser _idParser;

        private Store.FSDirectory _directory;

        private IndexSearcher _searcher;

        private IndexReader _reader;

        private IndexWriter _indexWriter;


        internal EntityExtractor(string indexPath)
        {
            _indexPath = indexPath;
            _directory = Store.FSDirectory.Open(_indexPath);
            _analyzer = new StandardAnalyzer(Version.LUCENE_30);

            _contentParser = new QueryParser(Version.LUCENE_30, "content", _analyzer);
            _idParser = new QueryParser(Version.LUCENE_30, "id", _analyzer);
        }

        public void StartFreebaseIndexRebuild()
        {
            if (Directory.Exists(_indexPath))
                //we are rebuilding the index from scratch
                Directory.Delete(_indexPath, true);

            _directory = Store.FSDirectory.Open(_indexPath);


            //create the index writer with the directory and analyzer defined.
            _indexWriter = new IndexWriter(_directory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }


        internal void AddEntry(string mid, IEnumerable<string> aliases, string description, IEnumerable<string> inIds, IEnumerable<string> outIds)
        {

            _indexWriter.AddDocument(getDocumentWithContent(description, mid, ContentCategory.D, inIds, outIds));
            var isLabel = true;
            foreach (var alias in aliases)
            {
                var sanitizedAlias = alias.Trim('"');
                var category = isLabel ? ContentCategory.L : ContentCategory.A;
                _indexWriter.AddDocument(getDocumentWithContent(sanitizedAlias, mid, category, inIds, outIds));
                isLabel = false; //first alias is considered to be a label
            }
        }

        public void FinishFreebaseIndexRebuild()
        {
            //optimize and close the writer
            _indexWriter.Optimize();
            _indexWriter.Dispose();
            _indexWriter = null;
        }

        internal void LoadIndex()
        {
            _reader = IndexReader.Open(_directory, false);

            _searcher = new IndexSearcher(_directory, false);
        }

        private Document getDocumentWithContent(string content, string mid, ContentCategory category, IEnumerable<string> inIds, IEnumerable<string> outIds)
        {
            var fldContentCategory = new Field("contentCategory", category.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);
            var fldContent = new Field("content", content.ToLowerInvariant(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);
            var fldId = new Field("id", GetFreebaseId(mid), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);
            var inBounds = inIds == null ? 0 : inIds.Count();
            var outBounds = outIds == null ? 0 : outIds.Count();

            var fldInBounds = new Field("inBounds", inBounds.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);

            var fldOutBounds = new Field("outBounds", outBounds.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);

            var doc = new Document();
            doc.Add(fldContent);
            doc.Add(fldId);
            doc.Add(fldContentCategory);
            doc.Add(fldInBounds);
            doc.Add(fldOutBounds);

            if (inIds != null)
                foreach (var inId in inIds)
                    doc.Add(new Field("inId", inId, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

            if (outIds != null)
                foreach (var outId in outIds)
                    doc.Add(new Field("outId", outId, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

            return doc;
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

                var scoredDocs = getScoredContentDocs(ngram);
                foreach (var dc in scoredDocs)
                {
                    var mid = getMid(dc);
                    var content = getContent(dc);
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
                        scores[mid] = entity = createEntity(mid, dc);
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

        internal string GetLabel(string mid)
        {
            var docs = getScoredIdDocs(mid);
            string label = null;
            foreach (var doc in docs)
            {
                var id = getMid(doc);
                if (id != mid)
                    continue;

                var content = getContent(doc);
                if (hasLabel(doc))
                    return content;

                if (label == null)
                    label = content;

                if (content.Length < label.Length)
                    label = content;
            }

            return label;
        }


        internal IEnumerable<string> GetAliases(string mid)
        {
            var docs = getScoredIdDocs(mid);
            var result = new List<string>();
            foreach (var doc in docs)
            {
                var type = getContentCategory(doc);
                if (type == ContentCategory.A)
                    result.Add(getContent(doc));
            }

            return result;
        }

        internal string GetDescription(string mid)
        {
            var docs = getScoredIdDocs(mid);
            foreach (var doc in docs)
            {
                var type = getContentCategory(doc);
                if (type == ContentCategory.D)
                    return getContent(doc);
            }

            return null;
        }

        internal IEnumerable<EntityInfo> GetEntities(string ngram)
        {
            var scores = new Dictionary<string, EntityInfo>();
            var scoredDocs = getScoredContentDocs(ngram);
            foreach (var dc in scoredDocs)
            {
                var mid = getMid(dc);
                var content = getContent(dc);
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
                    scores[mid] = entity = createEntity(mid, dc);
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

        private EntityInfo createEntity(string mid, ScoreDoc dc)
        {
            var label = GetLabel(mid);
            var doc = _searcher.Doc(dc.Doc);

            var inBounds = int.Parse(doc.GetField("inBounds").StringValue);
            var outBounds = int.Parse(doc.GetField("outBounds").StringValue);
            return new EntityInfo(mid, label, inBounds, outBounds);
        }

        private IEnumerable<ScoreDoc> getScoredContentDocs(string termVariant)
        {
            ScoreDoc[] docs;
            if (!_scoredDocsCache.TryGetValue(termVariant, out docs))
            {
                var queryStr = "\"" + QueryParser.Escape(termVariant) + "\"";
                var query = _contentParser.Parse(queryStr);

                var hits = _searcher.Search(query, 100);
                docs = hits.ScoreDocs.ToArray();
                _scoredDocsCache[termVariant] = docs;
            }

            return docs;
        }

        private IEnumerable<ScoreDoc> getScoredIdDocs(string mid)
        {
            var id = GetFreebaseId(mid);
            ScoreDoc[] docs;
            var queryStr = "\"" + QueryParser.Escape(id) + "\"";
            var query = _idParser.Parse(queryStr);

            var hits = _searcher.Search(query, 100);
            docs = hits.ScoreDocs.ToArray();
            _scoredDocsCache[mid] = docs;

            return docs;
        }

        internal string GetFreebaseId(string mid)
        {
            if (!mid.StartsWith(IdPrefix))
                throw new NotSupportedException("Invalid MID format");

            var id = mid.Substring(IdPrefix.Length);
            return id;
        }


        internal void Train(IEnumerable<string> ngrams, string correctAnswer)
        {
            string strongestNgram = null;
            string preBadNgram = null;
            var bestNgramScore = 0.0;

            foreach (var ngram in ngrams)
            {
                var docs = getScoredContentDocs(ngram);
                var bestDoc = docs.FirstOrDefault();
                if (bestDoc == null)
                    //nothing found for the ngram
                    continue;

                var id = getMid(bestDoc);
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

        private string getMid(ScoreDoc scoreDoc)
        {
            var doc = _searcher.Doc(scoreDoc.Doc);
            return IdPrefix + doc.GetField("id").StringValue;
        }

        private string getContent(ScoreDoc scoreDoc)
        {
            var doc = _searcher.Doc(scoreDoc.Doc);
            return doc.GetField("content").StringValue;
        }

        private bool hasLabel(ScoreDoc scoreDoc)
        {
            var doc = _searcher.Doc(scoreDoc.Doc);
            return getContentCategory(scoreDoc) == ContentCategory.L;
        }

        private ContentCategory getContentCategory(ScoreDoc scoreDoc)
        {
            var doc = _searcher.Doc(scoreDoc.Doc);
            switch (doc.GetField("contentCategory").StringValue)
            {
                case "L":
                    return ContentCategory.L;
                case "A":
                    return ContentCategory.A;
                case "D":
                    return ContentCategory.D;
                default:
                    throw new NotImplementedException();
            };
        }

        internal int GetInBounds(string mid)
        {
            foreach (var scoredDoc in getScoredIdDocs(mid))
            {
                var doc = _searcher.Doc(scoredDoc.Doc);
                return int.Parse(doc.GetField("inBounds").StringValue);
            }

            return 0;
        }

        internal int GetOutBounds(string mid)
        {
            foreach (var scoredDoc in getScoredIdDocs(mid))
            {
                var doc = _searcher.Doc(scoredDoc.Doc);
                return int.Parse(doc.GetField("outBounds").StringValue);
            }

            return 0;
        }
    }
}
