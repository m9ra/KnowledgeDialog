using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.Dataset
{
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

        private readonly Dictionary<string, DbPointer[]> _aliasIndex = new Dictionary<string, DbPointer[]>(StringComparer.InvariantCultureIgnoreCase);

        private readonly Dictionary<string, DbPointer> _idIndex = new Dictionary<string, DbPointer>();

        private readonly StreamReader _dbReader;

        internal FreebaseDbProvider2(string dbPath)
        {
            _dbReader = new StreamReader(dbPath);
            loadIndex(dbPath + ".index");
        }

        private void loadIndex(string path)
        {
            //TODO serialize index

            var currentPosition = 0u;
            while (!_dbReader.EndOfStream)
            {
                var pointer = new DbPointer(currentPosition);
                var line = _dbReader.ReadLine();
                currentPosition += (uint)(line.Length + 1);

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
