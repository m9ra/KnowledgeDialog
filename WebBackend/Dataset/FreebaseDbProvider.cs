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
using Neo4j.Driver.V1;



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
            //_analyzer = new KeywordAnalyzer();
            _contentParser = new QueryParser(Version.LUCENE_30, "content", _analyzer);

            var idAnalyzer = new KeywordAnalyzer();
            _idParser = new QueryParser(Version.LUCENE_30, "id", idAnalyzer);
        }

        internal void TestNeo4j()
        {
            using (var driver = GraphDatabase.Driver("bolt://localhost", AuthTokens.Basic("neo4j", "neo4jj")))
            using (var session = driver.Session())
            {
                session.Run("CREATE (a:Person {name:'Arthur', title:'King'})");
                var result = session.Run("MATCH (a:Person) WHERE a.name = 'Arthur' RETURN a.name AS name, a.title AS title");

                foreach (var record in result)
                {
                    foreach (var key in record.Keys)
                    {
                        Console.Write(record.Values[key]);
                        Console.Write(" ");
                    }
                    Console.WriteLine();
                }
            }
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

        internal void AddEntry(string mid, string label, IEnumerable<string> aliases, string description, IEnumerable<Tuple<string, string>> inTargets, IEnumerable<Tuple<string, string>> outTargets)
        {
            _indexWriter.AddDocument(getDocumentWithContent(label.Trim('"'), mid, ContentCategory.L, inTargets, outTargets));
            _indexWriter.AddDocument(getDocumentWithContent(description, mid, ContentCategory.D, null, null));

            foreach (var alias in aliases)
            {
                var sanitizedAlias = alias.Trim('"');
                var category = ContentCategory.A;
                _indexWriter.AddDocument(getDocumentWithContent(sanitizedAlias, mid, category, null, null));
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
                var contentCategory = GetContentCategory(doc);
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
            var entry = GetEntryFromId(FreebaseLoader.GetId(mid));
            return entry.Label;
        }

        internal IEnumerable<string> GetAliases(string mid)
        {
            var entry = GetEntryFromId(GetFreebaseId(mid));
            return entry.Aliases;
        }

        internal string GetDescription(string mid)
        {
            var entry = GetEntryFromId(GetFreebaseId(mid));
            return entry.Description;
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
            return GetContentCategory(scoreDoc) == ContentCategory.L;
        }

        internal ContentCategory GetContentCategory(ScoreDoc scoreDoc)
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
            var entry = GetEntryFromId(GetFreebaseId(mid));
            return entry.Targets.Where(e => !e.Item1.IsOutcoming).Count();
        }

        internal int GetOutBounds(string mid)
        {
            var entry = GetEntryFromId(GetFreebaseId(mid));
            return entry.Targets.Where(e => e.Item1.IsOutcoming).Count();
        }

        internal IEnumerable<ScoreDoc> GetScoredContentDocs(string termVariant)
        {
            ScoreDoc[] docs;
            if (!_scoredDocsCache.TryGetValue(termVariant, out docs))
            {
                var queryStr = "\"" + QueryParser.Escape(termVariant) + "\"";
                Query query;
                //  if (termVariant.Contains(' '))
                query = _contentParser.Parse(queryStr);
                //  else
                //      query = new FuzzyQuery(new Term("content", queryStr), 0.5f);

                var hits = _searcher.Search(query, 100);
                docs = hits.ScoreDocs.ToArray();
                _scoredDocsCache[termVariant] = docs;
            }

            return docs;
        }

        private IEnumerable<ScoreDoc> getScoredIdDocs(string id)
        {
            ScoreDoc[] docs;
            var query1 = new TermQuery(new Term("id", id));
            var boolQuery = new BooleanQuery();
            boolQuery.Add(query1, Occur.MUST);
            var hits = _searcher.Search(boolQuery, 100);
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

            var doc = new Document();
            doc.Add(fldContent);
            doc.Add(fldId);
            doc.Add(fldContentCategory);

            if (category == ContentCategory.L)
            {
                //targets will come only with label
                if (inTargets != null)
                    foreach (var inTarget in inTargets)
                        doc.Add(new Field("inTargets", inTarget.Item1 + ";" + inTarget.Item2, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

                if (outTargets != null)
                    foreach (var outTarget in outTargets)
                        doc.Add(new Field("outTargets", outTarget.Item1 + ";" + outTarget.Item2, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            }
            return doc;
        }


        internal EntityInfo GetEntityInfoFromMid(string mid)
        {
            var id = GetFreebaseId(mid);
            var entry = GetEntryFromId(id);

            return new EntityInfo(mid, entry.Label, entry.Targets.Where(t => !t.Item1.IsOutcoming).Count(), entry.Targets.Where(t => t.Item1.IsOutcoming).Count(), entry.Description);
        }
    }
}
