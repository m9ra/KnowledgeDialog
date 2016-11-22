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

        private readonly Dictionary<string, FreebaseEntry> _entryCache = new Dictionary<string, FreebaseEntry>();

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

            var idAnalyzer = new KeywordAnalyzer();
            _idParser = new QueryParser(Version.LUCENE_30, "id", idAnalyzer);
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

        internal void AddEntry(string mid, IEnumerable<string> aliases, string description, IEnumerable<Tuple<string, string>> inTargets, IEnumerable<Tuple<string, string>> outTargets)
        {
            _indexWriter.AddDocument(getDocumentWithContent(description, mid, ContentCategory.D, inTargets, outTargets));
            var isLabel = true;
            foreach (var alias in aliases)
            {
                var sanitizedAlias = alias.Trim('"');
                var category = isLabel ? ContentCategory.L : ContentCategory.A;
                _indexWriter.AddDocument(getDocumentWithContent(sanitizedAlias, mid, category, inTargets, outTargets));
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

        internal FreebaseEntry GetEntryFromId(string id)
        {
            FreebaseEntry entry;
            if (_entryCache.TryGetValue(id, out entry))
                return entry;

            var startTime = DateTime.Now;
            var docs = getScoredIdDocs(id);

            string label = null;
            string description = null;
            IEnumerable<Tuple<Edge, string>> targets = null;
            var aliases = new List<string>();
            foreach (var doc in docs)
            {
                var docId = GetId(doc);
                if (docId != id)
                    continue;

                if (targets == null)
                    targets = getTargets(doc);

                var content = GetContent(doc);
                var contentCategory = getContentCategory(doc);
                switch (contentCategory)
                {
                    case ContentCategory.A:
                        aliases.Add(content);
                        break;
                    case ContentCategory.D:
                        description = content;
                        break;
                    case ContentCategory.L:
                        label = content;
                        break;
                }
            }
            if (targets == null)
                targets = Enumerable.Empty<Tuple<Edge, string>>();

            entry = new FreebaseEntry(id, label, description, aliases, targets);
            _entryCache[id] = entry;

            var endTime = DateTime.Now;

            var duration = (endTime - startTime).TotalSeconds;
            //Console.WriteLine("DB Entry creation {0:0.000}s", duration);
            return entry;
        }

        private IEnumerable<Tuple<Edge, string>> getTargets(ScoreDoc scoreDoc)
        {
            var result = new List<Tuple<Edge, string>>();
            var doc = _searcher.Doc(scoreDoc.Doc);
            var inFields = doc.GetFields("inTargets");
            foreach (var inField in inFields)
            {
                var parsedInField = inField.StringValue.Split(';');
                result.Add(Tuple.Create(Edge.Incoming(parsedInField[0]), parsedInField[1]));
            }

            var outFields = doc.GetFields("outTargets");
            foreach (var outField in outFields)
            {
                var parsedOutField = outField.StringValue.Split(';');
                result.Add(Tuple.Create(Edge.Outcoming(parsedOutField[0]), parsedOutField[1]));
            }

            return result;
        }

        internal string GetLabel(string mid)
        {
            var docs = getScoredMidDocs(mid);
            string label = null;
            foreach (var doc in docs)
            {
                var docMid = GetMid(doc);
                if (docMid != mid)
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
            var docs = getScoredMidDocs(mid);
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
            var docs = getScoredMidDocs(mid);
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
            return IdPrefix + GetId(scoreDoc);
        }

        internal string GetId(ScoreDoc scoreDoc)
        {
            var doc = _searcher.Doc(scoreDoc.Doc);
            return doc.GetField("id").StringValue;
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
            foreach (var scoredDoc in getScoredMidDocs(mid))
            {
                var doc = _searcher.Doc(scoredDoc.Doc);
                return int.Parse(doc.GetField("inBounds").StringValue);
            }

            return 0;
        }

        internal int GetOutBounds(string mid)
        {
            foreach (var scoredDoc in getScoredMidDocs(mid))
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

        private IEnumerable<ScoreDoc> getScoredMidDocs(string mid)
        {
            var id = GetFreebaseId(mid);
            return getScoredIdDocs(id);
        }

        private IEnumerable<ScoreDoc> getScoredIdDocs(string id)
        {
            ScoreDoc[] docs;
            var queryStr = "\"" + QueryParser.Escape(id) + "\"";
            var query = _idParser.Parse(queryStr);

            var hits = _searcher.Search(query, 100);
            docs = hits.ScoreDocs.ToArray();

            return docs;
        }

        internal string GetFreebaseId(string mid)
        {
            if (!mid.StartsWith(IdPrefix))
                throw new NotSupportedException("Invalid MID format");

            var id = mid.Substring(IdPrefix.Length);
            return id;
        }

        private Document getDocumentWithContent(string content, string mid, ContentCategory category, IEnumerable<Tuple<string, string>> inTargets, IEnumerable<Tuple<string, string>> outTargets)
        {
            var fldContentCategory = new Field("contentCategory", category.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);
            var fldContent = new Field("content", content.ToLowerInvariant(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);
            var fldId = new Field("id", GetFreebaseId(mid), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);
            var inBounds = inTargets == null ? 0 : inTargets.Count();
            var outBounds = outTargets == null ? 0 : outTargets.Count();

            var fldInBounds = new Field("inBounds", inBounds.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);

            var fldOutBounds = new Field("outBounds", outBounds.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO);

            var doc = new Document();
            doc.Add(fldContent);
            doc.Add(fldId);
            doc.Add(fldContentCategory);
            doc.Add(fldInBounds);
            doc.Add(fldOutBounds);

            if (inTargets != null)
                foreach (var inTarget in inTargets)
                    doc.Add(new Field("inTargets", inTarget.Item1 + ";" + inTarget.Item2, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

            if (outTargets != null)
                foreach (var outTarget in outTargets)
                    doc.Add(new Field("outTargets", outTarget.Item1 + ";" + outTarget.Item2, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

            return doc;
        }

    }
}
