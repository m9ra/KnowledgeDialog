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
using KnowledgeDialog.Knowledge;


namespace WebBackend.AnswerExtraction
{

    class EntityInfo : IComparable<EntityInfo>
    {
        public readonly string Name = null;

        public readonly string Mid;

        public readonly double Score = 0;

        private readonly double _bestLabelScore = 0;

        internal EntityInfo(string mid)
        {
            Mid = mid;
        }

        private EntityInfo(string mid, string name, double score, double bestLabelScore)
        {
            Mid = mid;
            Name = name;
            Score = score;
            _bestLabelScore = bestLabelScore;
        }

        internal EntityInfo AddScore(string content, double score)
        {
            var isAlias = content.Length < 15;
            if (isAlias && score > _bestLabelScore)
            {
                return new EntityInfo(Mid, content, Score + score, score);
            }
            else
            {
                return new EntityInfo(Mid, Name, Score + score, _bestLabelScore);
            }
        }

        internal EntityInfo SubtractScore(double score)
        {
            return new EntityInfo(Mid, Name, Score - score, _bestLabelScore);
        }

        public override bool Equals(object obj)
        {
            var o = obj as EntityInfo;
            if (o == null)
                return false;

            return this.Mid.Equals(o.Mid);
        }

        public override int GetHashCode()
        {
            return Mid.GetHashCode();
        }

        public int CompareTo(EntityInfo other)
        {
            return Score.CompareTo(other.Score);
        }
    }

    class Extractor
    {
        internal static readonly string IdPrefix = "www.freebase.com/m/";

        /// <summary>
        /// Path to index directory.
        /// </summary>
        private readonly string _indexPath;

        private readonly Analyzer _analyzer;

        private readonly Dictionary<string, IEnumerable<string>> _documents = new Dictionary<string, IEnumerable<string>>();

        private readonly Dictionary<string, string> _documentDescriptions = new Dictionary<string, string>();

        private readonly Dictionary<string, double> _avgDocumentLengths = new Dictionary<string, double>();

        private readonly Dictionary<string, ScoreDoc[]> _scoredDocsCache = new Dictionary<string, ScoreDoc[]>();

        private readonly Dictionary<string, int> _badNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _preBadNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _leadingNgramCounts = new Dictionary<string, int>();

        private readonly QueryParser _contentParser;

        private readonly QueryParser _idParser;

        private Store.FSDirectory _directory;

        private IndexSearcher _searcher;

        private IndexReader _reader;


        internal Extractor(string indexPath)
        {
            _indexPath = indexPath;
            _directory = Store.FSDirectory.Open(_indexPath);
            _analyzer = new StandardAnalyzer(Version.LUCENE_30);

            _contentParser = new QueryParser(Version.LUCENE_30, "content", _analyzer);
            _idParser = new QueryParser(Version.LUCENE_30, "id", _analyzer);
        }

        public void RebuildFreebaseIndex()
        {
            if (Directory.Exists(_indexPath))
                //we are rebuilding the index from scratch
                Directory.Delete(_indexPath, true);

            _directory = Store.FSDirectory.Open(_indexPath);


            //create the index writer with the directory and analyzer defined.
            var indexWriter = new IndexWriter(_directory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED);

            //write the document to the index
            foreach (var id in _documents.Keys)
            {
                var aliasLengthSum = 0;
                var aliases = _documents[id];
                indexWriter.AddDocument(getDocumentWithContent(_documentDescriptions[id], id, false));
                var isLabel = true;
                foreach (var alias in aliases)
                {
                    var sanitizedAlias = alias.Trim('"');
                    indexWriter.AddDocument(getDocumentWithContent(sanitizedAlias, id, isLabel));
                    isLabel = false; //first alias is considered to be a label
                    aliasLengthSum += sanitizedAlias.Length;
                }

                _avgDocumentLengths[id] = 1.0 * aliasLengthSum / aliases.Count();
            }

            //optimize and close the writer
            indexWriter.Optimize();
            indexWriter.Dispose();

            LoadIndex();
        }

        internal void LoadIndex()
        {
            _reader = IndexReader.Open(_directory, false);

            _searcher = new IndexSearcher(_directory, false);
        }

        private Document getDocumentWithContent(string content, string id, bool isLabel)
        {
            var isLabelStr = isLabel ? "T" : "F";
            var fldIsLabel = new Field("isLabel", isLabelStr, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);
            var fldContent = new Field("content", content.ToLowerInvariant(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);
            var fldId = new Field("id", GetFreebaseId(id), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);

            var doc = new Document();
            doc.Add(fldContent);
            doc.Add(fldId);
            doc.Add(fldIsLabel);
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
                    var id = getId(dc);
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
                    if (!scores.TryGetValue(id, out entity))
                    {
                        scores[id] = entity = new EntityInfo(id);
                    }
                    scores[id] = entity.AddScore(content, score);
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
                var id = getId(doc);
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

        private IEnumerable<ScoreDoc> getScoredIdDocs(string id)
        {
            var mid = GetFreebaseId(id);
            ScoreDoc[] docs;
            var queryStr = "\"" + QueryParser.Escape(mid) + "\"";
            var query = _idParser.Parse(queryStr);

            var hits = _searcher.Search(query, 100);
            docs = hits.ScoreDocs.ToArray();
            _scoredDocsCache[id] = docs;

            return docs;
        }

        internal string GetFreebaseId(string mid)
        {
            if (!mid.StartsWith(IdPrefix))
                throw new NotSupportedException("Invalid MID format");

            var id = mid.Substring(IdPrefix.Length);
            return id;
        }

        internal void AddEntry(string freebaseId, IEnumerable<string> aliases, string description)
        {
            _documents[freebaseId] = aliases;
            _documentDescriptions[freebaseId] = description;
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

                var id = getId(bestDoc);
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

        private string getId(ScoreDoc scoreDoc)
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
            return doc.GetField("isLabel").StringValue == "T";
        }
    }
}
