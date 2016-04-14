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
                    indexWriter.AddDocument(getDocumentWithContent(alias, id));
                    aliasLengthSum += alias.Length;
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
            var fldContent = new Field("content", content, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);

            var fldId = new Field("id", id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);
            var doc = new Document();
            doc.Add(fldContent);
            doc.Add(fldId);
            return doc;
        }

        public string Find(IEnumerable<string> ngrams, IEnumerable<string> contextNgrams)
        {
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
                    var score = dc.Score;
                    score = score * ngram.Length;

                    double actualScore;
                    scores.TryGetValue(id, out actualScore);
                    scores[id] = actualScore + score;
                }
            }
            /*
            foreach (var contextNgram in contextNgrams)
            {
                int badFactor;
                _trainBadNgramCounts.TryGetValue(contextNgram, out badFactor);
                if (badFactor > 0)
                    continue;

                var scoredDocs = getScoredDocs(contextNgram);
                foreach (var dc in scoredDocs)
                {
                    var id = getId(dc);
                    var content = getContent(dc);
                    var score = dc.Score;
                    score = score * contextNgram.Length;

                    double actualScore;
                    scores.TryGetValue(id, out actualScore);
                    scores[id] = 0;
                }
            }
            */
            if (scores.Count == 0)
                return null;

            var maxValue = scores.Values.Max();
            return scores.Where(p => p.Value == maxValue).Select(p => p.Key).FirstOrDefault();
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
            foreach (var ngram in ngrams)
            {
                var docs = getScoredDocs(ngram);
                var bestDoc = docs.FirstOrDefault();
                if (bestDoc == null)
                    //nothing found for the ngram
                    continue;

                var id = getId(bestDoc);
                if (id == correctAnswer)
                    //the ngram is helpful
                    continue;

                int count;
                _trainBadNgramCounts.TryGetValue(ngram, out count);
                _trainBadNgramCounts[ngram] = count + 1;
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
