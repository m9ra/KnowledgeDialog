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
    public class SampleData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    class Extractor
    {
        /// <summary>
        /// Path to index directory.
        /// </summary>
        private readonly string _indexPath = @".\lucene_index";

        private readonly Analyzer _analyzer;

        private readonly Dictionary<string, IEnumerable<string>> _documents = new Dictionary<string, IEnumerable<string>>();

        private readonly Dictionary<string, string> _documentDescriptions = new Dictionary<string, string>();

        private readonly Dictionary<string, double> _avgDocumentLengths = new Dictionary<string, double>();

        private readonly Dictionary<string, ScoreDoc[]> _scoredDocsCache = new Dictionary<string, ScoreDoc[]>();

        private readonly Dictionary<string, int> _trainBadNgramCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _leadingNgramCounts = new Dictionary<string, int>();

        private readonly QueryParser _parser;

        private Store.FSDirectory _directory;

        private IndexSearcher _searcher;

        private IndexReader _reader;

        private bool _isTrained = false;


        internal Extractor()
        {
            _directory = Store.FSDirectory.Open(_indexPath);
            _analyzer = new StandardAnalyzer(Version.LUCENE_30);

            _parser = new QueryParser(Version.LUCENE_30, "content", _analyzer);
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
                indexWriter.AddDocument(getDocumentWithContent(_documentDescriptions[id], id));
                foreach (var alias in aliases)
                {
                    var sanitizedAlias = alias.Trim('"');
                    indexWriter.AddDocument(getDocumentWithContent(sanitizedAlias, id));
                    aliasLengthSum += sanitizedAlias.Length;
                }

                _avgDocumentLengths[id] = 1.0 * aliasLengthSum / aliases.Count();
            }

            //optimize and close the writer
            indexWriter.Optimize();
            indexWriter.Dispose();

            _reader = IndexReader.Open(_directory, false);

            _searcher = new IndexSearcher(_directory, false);
        }

        private Document getDocumentWithContent(string content, string id)
        {
            var fldContent = new Field("content", content.ToLowerInvariant(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);

            var fldId = new Field("id", id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);
            var doc = new Document();
            doc.Add(fldContent);
            doc.Add(fldId);
            return doc;
        }

        private Dictionary<string, double> rawScores(string[] ngrams, int aliasLength, double leadingScoreFactor)
        {
            int leadingScore = 0;
            var scores = new Dictionary<string, double>();

            foreach (var ngram in ngrams)
            {
                int badFactor;
                _trainBadNgramCounts.TryGetValue(ngram, out badFactor);
                if (badFactor > 0)
                    continue;

                var scoredDocs = getScoredDocs(ngram);
                foreach (var dc in scoredDocs)
                {
                    var id = getId(dc);
                    var content = getContent(dc);
                    var isAlias = content.Length < aliasLength;
                    var score = dc.Score;
                    score = score * ngram.Length;
                    score = score + leadingScore * (float)leadingScoreFactor;

                    if (isAlias)
                    {
                        var lengthDiff = Math.Abs(content.Length - ngram.Length);
                        score = score / content.Length * 2;
                    }
                    else
                    {
                        score = score / 10;
                    }

                    double actualScore;
                    scores.TryGetValue(id, out actualScore);
                    scores[id] = actualScore + score;
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

        internal Dictionary<string, double> RawScores(string[] ngrams)
        {
            var aliasLength = 15;
            return rawScores(ngrams, aliasLength, 3);
        }

        internal Ranked<string>[] Score(string[] ngrams, string[] contextNgrams)
        {
            var scores = RawScores(ngrams);
            if (scores.Count == 0)
                return new Ranked<string>[0];

            var badScores = rawScores(contextNgrams, 15, 0.0);
            foreach (var badScore in badScores)
            {
               // break;
                if (scores.ContainsKey(badScore.Key))
                    scores[badScore.Key] -= badScore.Value / 10;
            }

            var rankedAnswers = new List<Ranked<string>>();
            foreach (var pair in scores)
            {
                var rank = new Ranked<string>(pair.Key, pair.Value);
                rankedAnswers.Add(rank);
            }

            rankedAnswers.Sort();
            return rankedAnswers.ToArray();
        }

        public string Find(IEnumerable<string> ngrams, IEnumerable<string> contextNgrams)
        {
            var bestScore = Score(ngrams.ToArray(), contextNgrams.ToArray()).FirstOrDefault();
            if (bestScore == null)
                return null;

            return bestScore.Value;
        }

        private IEnumerable<ScoreDoc> getScoredDocs(string termVariant)
        {
            ScoreDoc[] docs;
            if (!_scoredDocsCache.TryGetValue(termVariant, out docs))
            {
                var queryStr = "\"" + QueryParser.Escape(termVariant) + "\"";
                var query = _parser.Parse(queryStr);

                var hits = _searcher.Search(query, 100);
                docs = hits.ScoreDocs.ToArray();
                _scoredDocsCache[termVariant] = docs;
            }

            return docs;
        }

        internal void AddEntry(string freebaseId, IEnumerable<string> aliases, string description)
        {
            _documents[freebaseId] = aliases;
            _documentDescriptions[freebaseId] = description;
        }

        internal void Train(IEnumerable<string> ngrams, string correctAnswer)
        {
            string strongestNgram = null;
            var bestNgramScore = 0.0;

            foreach (var ngram in ngrams)
            {
                var docs = getScoredDocs(ngram);
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
                    _trainBadNgramCounts.TryGetValue(ngram, out count);
                    _trainBadNgramCounts[ngram] = count + 1;
                }
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
            return doc.GetField("id").StringValue;
        }

        private string getContent(ScoreDoc scoreDoc)
        {
            var doc = _searcher.Doc(scoreDoc.Doc);
            return doc.GetField("content").StringValue;
        }
    }
}
