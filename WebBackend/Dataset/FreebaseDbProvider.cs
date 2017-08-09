using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.IO;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.Dataset
{
    [Serializable]
    internal struct DbPointer
    {
        internal double Score { get { return 1.0; } }

        internal uint Offset;

        internal DbPointer(uint offset)
        {
            Offset = offset;
        }
    }

    class FreebaseDbProvider
    {
        internal static readonly string EdgePrefix = "www.freebase.com";

        internal static readonly string IdPrefix = "www.freebase.com/m/";

        internal static string EnglishSuffix = "@en";

        private readonly Dictionary<DbPointer, FreebaseEntry> _entryCache = new Dictionary<DbPointer, FreebaseEntry>();

        private Dictionary<string, DbPointer[]> _aliasIndex = new Dictionary<string, DbPointer[]>(StringComparer.InvariantCultureIgnoreCase);

        private Dictionary<string, List<DbPointer>> _valueConfusions;

        private Dictionary<string, DbPointer> _idIndex = new Dictionary<string, DbPointer>();

        private readonly StreamReader _dbReader;


        internal FreebaseDbProvider(string dbPath)
        {
            _dbReader = new StreamReader(dbPath, Encoding.UTF8, false, 4096 * 2);

            loadIndex(dbPath + ".index");
            loadConfusions();
        }

        internal FreebaseEntry GetEntryFromMid(string mid)
        {
            return GetEntryFromId(GetId(mid));
        }

        internal static string GetId(string mid)
        {
            if (!mid.StartsWith(IdPrefix))
                throw new NotSupportedException("Mid format unknown: " + mid);

            return mid.Substring(IdPrefix.Length);
        }

        internal static string GetMid(string id)
        {
            if (id.StartsWith(IdPrefix))
                throw new NotImplementedException("Id prefix is already present." + id);

            return IdPrefix + id;
        }

        internal static string GetShortEdgeName(string edgeId)
        {
            if (!edgeId.StartsWith(EdgePrefix))
                throw new NotSupportedException("Edge format unknown: " + edgeId);

            return edgeId.Substring(edgeId.Length);
        }

        internal static string TryGetId(string identifier)
        {
            if (identifier.StartsWith(IdPrefix))
                return GetMid(identifier);

            return identifier;
        }


        internal FreebaseEntry GetEntryFromId(string id)
        {
            DbPointer pointer;
            if (!_idIndex.TryGetValue(id, out pointer))
                // entity is not available
                return null;

            return GetEntry(pointer);
        }

        internal FreebaseEntry GetEntry(DbPointer pointer)
        {
            FreebaseEntry entry;
            if (!_entryCache.TryGetValue(pointer, out entry))
                _entryCache[pointer] = entry = loadEntry(pointer);

            return entry;
        }

        private FreebaseEntry loadEntry(DbPointer pointer)
        {
            _dbReader.BaseStream.Seek(pointer.Offset, SeekOrigin.Begin);
            _dbReader.DiscardBufferedData();
            var entryLine = _dbReader.ReadLine();

            return DumpLoader.ParseEntry(entryLine);
        }

        internal string GetLabel(string mid)
        {
            var entry = GetEntryFromId(FreebaseDbProvider.GetId(mid));
            if (entry == null)
                return null;
            return entry.Label;
        }

        internal IEnumerable<string> GetAliases(string mid)
        {
            var entry = GetEntryFromId(GetFreebaseId(mid));
            return entry.Aliases;
        }


        internal IEnumerable<string> GetNames(string id)
        {
            var entry = GetEntryFromId(id);
            if (entry == null)
                return new string[0];
            return new[] { entry.Label }.Concat(entry.Aliases);
        }

        internal IEnumerable<string> GetNamesFromMid(string mid)
        {
            return GetNames(GetId(mid));
        }

        internal string GetDescription(string mid)
        {
            var entry = GetEntryFromId(GetFreebaseId(mid));
            return entry.Description;
        }

        internal string GetMid(DbPointer pointer)
        {
            return IdPrefix + GetId(pointer);
        }

        internal string GetId(DbPointer pointer)
        {
            return GetEntry(pointer).Id;
        }

        internal int GetInBounds(string mid)
        {
            var entry = GetEntryFromId(GetFreebaseId(mid));
            if (entry == null)
                return 0;

            return entry.Targets.Where(e => !e.Item1.IsOutcoming).Count();
        }

        internal int GetOutBounds(string mid)
        {
            var entry = GetEntryFromId(GetFreebaseId(mid));
            if (entry == null)
                return 0;

            return entry.Targets.Where(e => e.Item1.IsOutcoming).Count();
        }

        internal IEnumerable<DbPointer> GetScoredDocs(string termVariant)
        {
            DbPointer[] pointers;

            var result = new HashSet<DbPointer>();
            if (_aliasIndex.TryGetValue(termVariant, out pointers))
                result.UnionWith(pointers);

            termVariant = sanitizeName(termVariant);
            List<DbPointer> pointers2;
            if (_valueConfusions.TryGetValue(termVariant, out pointers2))
                result.UnionWith(pointers2);

            return result;
        }

        internal string GetFreebaseId(string mid)
        {
            if (!mid.StartsWith(IdPrefix))
                throw new NotSupportedException("Invalid MID format");

            var id = mid.Substring(IdPrefix.Length);
            return id;
        }

        internal EntityInfo GetEntity(DbPointer pointer)
        {
            var entry = GetEntry(pointer);

            return new EntityInfo(FreebaseDbProvider.GetMid(entry.Id), entry.Label, entry.Targets.Where(t => !t.Item1.IsOutcoming).Count(), entry.Targets.Where(t => t.Item1.IsOutcoming).Count(), entry.Description);
        }

        internal EntityInfo GetEntityInfoFromMid(string mid)
        {
            var id = GetFreebaseId(mid);
            var entry = GetEntryFromId(id);
            if (entry == null)
                return null;

            return new EntityInfo(mid, entry.Label, entry.Targets.Where(t => !t.Item1.IsOutcoming).Count(), entry.Targets.Where(t => t.Item1.IsOutcoming).Count(), entry.Description);
        }

        internal bool ContainsId(string id)
        {
            return _idIndex.ContainsKey(id);
        }

        #region Index utilities

        private void loadConfusions()
        {
            _valueConfusions = new Dictionary<string, List<DbPointer>>(_aliasIndex.Count, StringComparer.InvariantCultureIgnoreCase);

            foreach (var pair in _aliasIndex)
            {
                var name = pair.Key;
                var confusedValue = sanitizeName(name);

                if (confusedValue != name)
                {
                    List<DbPointer> pointers;
                    if (!_valueConfusions.TryGetValue(confusedValue, out pointers))
                        _valueConfusions[confusedValue] = pointers = new List<DbPointer>();

                    pointers.AddRange(pair.Value);
                }
            }
        }

        private void loadIndex(string indexPath)
        {
            if (File.Exists(indexPath + "._idIndex"))
            {
                Console.WriteLine("Loading index");
                _idIndex = deserializePointers(indexPath, "_idIndex");
                _aliasIndex = deserializePointerArrays(indexPath, "_aliasIndex", StringComparer.InvariantCultureIgnoreCase);
                Console.WriteLine("\tfinished.");
                return;
            }

            Console.WriteLine("DB index creation");
            var currentPosition = 0u;
            while (!_dbReader.EndOfStream)
            {
                var pointer = new DbPointer(currentPosition);
                var line = _dbReader.ReadLine();
                var lineLength = _dbReader.CurrentEncoding.GetBytes(line).Length;
                currentPosition += (uint)(lineLength + 1);

                string id;
                var aliases = DumpLoader.ParseLabels(line, out id);
                foreach (var alias in aliases)
                {
                    if (_aliasIndex.ContainsKey(alias))
                        _aliasIndex[alias] = _aliasIndex[alias].Concat(new[] { pointer }).ToArray();
                    else
                        _aliasIndex[alias] = new[] { pointer };
                }

                _idIndex[id] = pointer;
            }
            Console.WriteLine("\tserializing");
            serialize(indexPath, "_aliasIndex", _aliasIndex);
            serialize(indexPath, "_idIndex", _idIndex);
            Console.WriteLine("\tfinished.");
        }

        public void serialize(string dbFile, string index, Dictionary<string, DbPointer> dictionary)
        {
            var file = dbFile + "." + index;
            using (var writer = new BinaryWriter(new FileStream(file, FileMode.Create)))
            {
                writer.Write(dictionary.Count);
                foreach (var kvp in dictionary)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value.Offset);
                }
                writer.Flush();
                writer.Close();
            }
        }

        public Dictionary<string, DbPointer> deserializePointers(string dbFile, string index)
        {
            var file = dbFile + "." + index;
            using (var reader = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                var count = reader.ReadInt32();
                var dictionary = new Dictionary<string, DbPointer>(count);
                for (int n = 0; n < count; n++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadUInt32();
                    dictionary.Add(key, new DbPointer(value));
                }
                return dictionary;
            }
        }

        public void serialize(string dbFile, string index, Dictionary<string, DbPointer[]> dictionary)
        {
            var file = dbFile + "." + index;
            using (var writer = new BinaryWriter(new FileStream(file, FileMode.Create)))
            {
                writer.Write(dictionary.Count);
                foreach (var kvp in dictionary)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value.Length);
                    for (var i = 0; i < kvp.Value.Length; ++i)
                    {
                        writer.Write(kvp.Value[i].Offset);
                    }
                }
                writer.Flush();
                writer.Close();
            }
        }

        public Dictionary<string, DbPointer[]> deserializePointerArrays(string dbFile, string index, StringComparer comparer = null)
        {
            var file = dbFile + "." + index;
            using (var reader = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                var count = reader.ReadInt32();
                var dictionary = new Dictionary<string, DbPointer[]>(count, comparer);
                for (int n = 0; n < count; n++)
                {
                    var key = reader.ReadString();
                    var arraySize = reader.ReadInt32();
                    var array = new DbPointer[arraySize];
                    for (var i = 0; i < array.Length; ++i)
                    {
                        array[i] = new DbPointer(reader.ReadUInt32());
                    }
                    dictionary.Add(key, array);
                }
                return dictionary;
            }
        }

        private string sanitizeName(string name)
        {
            var sanitized = name.Replace('.', ' ').Replace('!', ' ').Replace('?', ' ').Replace(':', ' ').Replace(',', ' ').Replace('\'', ' ').Replace('"', ' ').Replace('/', ' ').Replace('\\', ' ').Trim();

            while (sanitized.Contains("  "))
                sanitized = sanitized.Replace("  ", " ");

            return sanitized;
        }

        #endregion
    }
}
