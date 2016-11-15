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

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.Dataset
{
    internal enum ContentCategory { L, A, D };

    class FreebaseDbProvider
    {
        internal static readonly string IdPrefix = "www.freebase.com/m/";

        private readonly Dictionary<string, ScoreDoc[]> _scoredDocsCache = new Dictionary<string, ScoreDoc[]>();


        /// <summary>
        /// Path to index directory.
        /// </summary>
        private readonly string _indexPath;

        private readonly Analyzer _analyzer;

        private readonly QueryParser _contentParser;

        private readonly QueryParser _idParser;

        private Store.FSDirectory _directory;

        private IndexSearcher _searcher;

        private IndexReader _reader;

        private IndexWriter _indexWriter;

        internal FreebaseDbProvider(string indexPath)
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

        internal EntityInfo CreateEntity(string mid, ScoreDoc dc)
        {
            var label = GetLabel(mid);
            var doc = _searcher.Doc(dc.Doc);

            var inBounds = int.Parse(doc.GetField("inBounds").StringValue);
            var outBounds = int.Parse(doc.GetField("outBounds").StringValue);
            return new EntityInfo(mid, label, inBounds, outBounds);
        }

        internal string GetLabel(string mid)
        {
            var docs = getScoredIdDocs(mid);
            string label = null;
            foreach (var doc in docs)
            {
                var id = GetMid(doc);
                if (id != mid)
                    continue;

                var content = GetContent(doc);
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
                    result.Add(GetContent(doc));
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
                    return GetContent(doc);
            }

            return null;
        }


        internal string GetMid(ScoreDoc scoreDoc)
        {
            var doc = _searcher.Doc(scoreDoc.Doc);
            return IdPrefix + doc.GetField("id").StringValue;
        }

        internal string GetContent(ScoreDoc scoreDoc)
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


        internal IEnumerable<ScoreDoc> GetScoredContentDocs(string termVariant)
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

    }
}
