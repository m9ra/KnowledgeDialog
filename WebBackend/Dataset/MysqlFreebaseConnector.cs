using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

using ServeRick.Modules.MySQL;

namespace WebBackend.Dataset
{
    class MysqlFreebaseConnector
    {
        internal readonly int IdLengthLimit = 15;

        internal readonly int AutoFlushEntityThreshold = 1000;

        internal readonly int AutoFlushEdgeThreshold = 1024 * 1024;

        private int _edgeDataLength = 0;

        private readonly MySqlConnection _connection;

        private readonly Dictionary<string, FreebaseEntity> _writeEntityCache = new Dictionary<string, FreebaseEntity>();

        private readonly List<Tuple<string, string, string>> _writeEdgeCache = new List<Tuple<string, string, string>>();

        internal MysqlFreebaseConnector()
        {
            var myConnectionString = "server=127.0.0.1;uid=root;pwd=12345;database=knowledge";
            _connection = new MySqlConnection();
            _connection.ConnectionString = myConnectionString;
            _connection.Open();

            InitializeDB();
        }

        internal void InitializeDB()
        {
            var query = getQuery();
            executeNonQuery(@"
CREATE TABLE IF NOT EXISTS `freebase_edges`
(    
    id_from VARCHAR(" + IdLengthLimit + @") NOT NULL,
    edge VARCHAR(120) NOT NULL,
    id_to VARCHAR(" + IdLengthLimit + @") NOT NULL,
    PRIMARY KEY(id_from,edge,id_to),
    INDEX(id_from),
    INDEX(id_to)    
)ENGINE = MyISAM;
");

            executeNonQuery(@"
CREATE TABLE IF NOT EXISTS `freebase_nodes`
(
    id VARCHAR(" + IdLengthLimit + @") NOT NULL PRIMARY KEY,
    label VARCHAR(60),
    description TEXT
)ENGINE = MyISAM;
");

            executeNonQuery(@"
CREATE TABLE IF NOT EXISTS `freebase_aliases`
(
    id VARCHAR(" + IdLengthLimit + @") NOT NULL,
    alias VARCHAR(60) NOT NULL,
    PRIMARY KEY(id,alias)
)ENGINE = MyISAM;
");
        }

        internal void WriteEntityInfo(string id, string label, string description, string alias)
        {
            var entity = getWriteEntityCache(id);
            if (label != null)
                entity.Label = label;

            if (description != null)
                entity.Description = description;

            if (alias != null)
                entity.Aliases.Add(alias);
        }

        internal void WriteEntityEdges(string id, string edge, IEnumerable<string> targetIds)
        {
            foreach (var targetId in targetIds)
            {
                _writeEdgeCache.Add(Tuple.Create(id, edge, targetId));
                _edgeDataLength += id.Length + edge.Length + targetId.Length;

                if (_edgeDataLength > AutoFlushEdgeThreshold)
                {
                    FlushWrites();
                    _edgeDataLength = 0;
                }
            }
        }

        internal void FlushWrites()
        {
            var entities = _writeEntityCache.Values;
            var completeNodeWrites = entities.Where(e => e.Label != null && e.Description != null);
            var descriptionNodeWrites = entities.Where(e => e.Label == null && e.Description != null);
            var labelNodeWrites = entities.Where(e => e.Label != null && e.Description == null);

            insertNodes(completeNodeWrites, (e) => new[] { e.Label, e.Description }, "label", "description");
            insertNodes(descriptionNodeWrites, (e) => new[] { e.Description }, "description");
            insertNodes(labelNodeWrites, (e) => new[] { e.Label }, "label");

            insertAliases(entities);
            insertEdges(_writeEdgeCache);

            _writeEntityCache.Clear();
            _writeEdgeCache.Clear();
        }

        private void insertNodes(IEnumerable<FreebaseEntity> entities, Func<FreebaseEntity, string[]> valueProcessor, params string[] columns)
        {
            if (!entities.Any())
                //there is nothing to do
                return;

            var sql = getQuery();
            sql.Append("INSERT INTO freebase_nodes(id," + string.Join(",", columns) + ") VALUES");
            var rowCount = 0;
            foreach (var entity in entities)
            {
                if (rowCount > 0)
                    sql.Append(",");

                sql.Append("(");
                var idParameter = sql.AddParameter("id" + rowCount, entity.FreebaseId);
                sql.Append(idParameter);
                var values = valueProcessor(entity);
                for (var i = 0; i < columns.Length; ++i)
                {
                    var value = values[i];
                    var column = columns[i];
                    sql.Append(",");
                    sql.Append(sql.AddParameter(column + rowCount, value));
                }
                ++rowCount;
                sql.Append(")");
            }

            sql.Append("ON DUPLICATE KEY UPDATE id=id");
            sql.ExecuteNonQuery();
        }

        private void insertEdges(IEnumerable<Tuple<string, string, string>> edges)
        {
            if (!edges.Any())
                //there is nothing to do
                return;

            var sql = getQuery();
            sql.Append("INSERT INTO freebase_edges(id_from,edge,id_to) VALUES");
            var rowCount = 0;
            foreach (var alias in edges)
            {
                if (rowCount > 0)
                    sql.Append(",");

                sql.AppendFormat("({0},{1},{2})",
                    sql.AddParameter("from" + rowCount, alias.Item1),
                    sql.AddParameter("edge" + rowCount, alias.Item2),
                    sql.AddParameter("to" + rowCount, alias.Item3)
                    );
                ++rowCount;
            }

            sql.Append("ON DUPLICATE KEY UPDATE id_from=id_from");
            sql.ExecuteNonQuery();
        }

        private void insertAliases(IEnumerable<FreebaseEntity> entities)
        {
            var aliases = new List<Tuple<string, string>>();
            foreach (var entity in entities)
            {
                foreach (var alias in entity.Aliases)
                {
                    aliases.Add(Tuple.Create(entity.FreebaseId, alias));
                }
            }

            if (aliases.Count == 0)
                return;

            var sql = getQuery();
            sql.Append("INSERT INTO freebase_aliases(id,alias) VALUES");
            var rowCount = 0;
            foreach (var alias in aliases)
            {
                if (rowCount > 0)
                    sql.Append(",");

                sql.AppendFormat("({0},{1})", sql.AddParameter("id" + rowCount, alias.Item1), sql.AddParameter("alias" + rowCount, alias.Item2));
                ++rowCount;
            }

            sql.Append("ON DUPLICATE KEY UPDATE id=id");
            sql.ExecuteNonQuery();
        }

        private FreebaseEntity getWriteEntityCache(string id)
        {
            FreebaseEntity result;
            if (!_writeEntityCache.TryGetValue(id, out result))
            {
                if (_writeEntityCache.Count > AutoFlushEntityThreshold)
                {
                    FlushWrites();
                }

                if (id.Length > IdLengthLimit)
                    Console.WriteLine("[WARNING] Id length: " + id);

                _writeEntityCache[id] = result = new FreebaseEntity(id);

            }

            return result;
        }

        private void executeNonQuery(string query)
        {
            var sqlQuery = getQuery();
            sqlQuery.Append(query);

            sqlQuery.ExecuteNonQuery();
        }

        /// <summary>
        /// Blockingly get command (with possible reconnection)
        /// </summary>
        /// <returns>Created command</returns>
        private SqlQuery getQuery()
        {
            var cmd = new SqlQuery(_connection);
            return cmd;
        }
    }
}
