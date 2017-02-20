﻿using System;
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
        internal uint Offset;

        internal DbPointer(uint offset)
        {
            Offset = offset;
        }
    }

    class FreebaseDbProvider2
    {
        internal static readonly string IdPrefix = "www.freebase.com/m/";

        private readonly Dictionary<DbPointer, FreebaseEntry> _entryCache = new Dictionary<DbPointer, FreebaseEntry>();

        private Dictionary<string, DbPointer[]> _aliasIndex = new Dictionary<string, DbPointer[]>(StringComparer.InvariantCultureIgnoreCase);

        private Dictionary<string, DbPointer> _idIndex = new Dictionary<string, DbPointer>();

        private readonly StreamReader _dbReader;

        internal FreebaseDbProvider2(string dbPath)
        {
            _dbReader = new StreamReader(dbPath);
            loadIndex(dbPath + ".index");
        }

        private void loadIndex(string indexPath)
        {
            if (File.Exists(indexPath + "._idIndex"))
            {
                Console.WriteLine("Loading index");
                _idIndex = DeserializePointers(indexPath, "_idIndex");
                _aliasIndex = DeserializePointerArrays(indexPath, "_aliasIndex");
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
            Serialize(indexPath, "_aliasIndex", _aliasIndex);
            Serialize(indexPath, "_idIndex", _idIndex);
            Console.WriteLine("\tfinished.");
        }

        public void Serialize(string dbFile, string index, Dictionary<string, DbPointer> dictionary)
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

        public Dictionary<string, DbPointer> DeserializePointers(string dbFile, string index)
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

        public void Serialize(string dbFile, string index, Dictionary<string, DbPointer[]> dictionary)
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

        public Dictionary<string, DbPointer[]> DeserializePointerArrays(string dbFile, string index)
        {
            var file = dbFile + "." + index;
            using (var reader = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                var count = reader.ReadInt32();
                var dictionary = new Dictionary<string, DbPointer[]>(count);
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
            return entry.Targets.Where(e => !e.Item1.IsOutcoming).Count();
        }

        internal int GetOutBounds(string mid)
        {
            var entry = GetEntryFromId(GetFreebaseId(mid));
            return entry.Targets.Where(e => e.Item1.IsOutcoming).Count();
        }

        internal IEnumerable<DbPointer> GetScoredContentDocs(string termVariant)
        {
            DbPointer[] pointers;

            if (_aliasIndex.TryGetValue(termVariant, out pointers))
                return pointers;

            return Enumerable.Empty<DbPointer>();
        }

        internal string GetFreebaseId(string mid)
        {
            if (!mid.StartsWith(IdPrefix))
                throw new NotSupportedException("Invalid MID format");

            var id = mid.Substring(IdPrefix.Length);
            return id;
        }

        internal EntityInfo GetEntityInfoFromMid(string mid)
        {
            var id = GetFreebaseId(mid);
            var entry = GetEntryFromId(id);

            return new EntityInfo(mid, entry.Label, entry.Targets.Where(t => !t.Item1.IsOutcoming).Count(), entry.Targets.Where(t => t.Item1.IsOutcoming).Count(), entry.Description);
        }
    }
}
