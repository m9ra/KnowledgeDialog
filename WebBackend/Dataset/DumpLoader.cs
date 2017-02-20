using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Dataset
{
    class DumpLoader
    {
        /// <summary>
        /// Path to the dump.
        /// </summary>
        private readonly string _dumpPath;

        internal DumpLoader(string dumpPath)
        {
            _dumpPath = dumpPath;
        }

        internal IEnumerable<FreebaseEntry> ReadDb()
        {
            using (var fileStream = new FileStream(_dumpPath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    yield return ParseEntry(line);
                }
            }
        }

        internal static FreebaseEntry ParseEntry(string line)
        {
            var lineParts = line.Split(new[] { '\t' }, 5);

            var freebaseId = lineParts[0];
            var aliasesStr = lineParts[1];

            var inEdgesStr = lineParts[2];
            var outEdgesStr = lineParts[3];
            var description = lineParts[4];

            var aliases = aliasesStr.Split(';');
            var label = aliases[0];


            var targets = parseEdges(inEdgesStr, false).Concat(parseEdges(outEdgesStr, true));
            return new FreebaseEntry(freebaseId, label, description, aliases.Skip(1), targets);
        }

        internal static IEnumerable<string> ParseLabels(string line, out string id)
        {
            var lineParts = line.Split('\t');
            id = lineParts[0];
            return lineParts[1].Split(';');
        }

        private static IEnumerable<Tuple<Edge, string>> parseEdges(string edgeStr, bool isOutcoming)
        {
            var edgeAssignments = edgeStr.Split(';');
            foreach (var edgeAssignment in edgeAssignments)
            {
                var parts = edgeAssignment.Split(new[] { ':' }, 2);
                var edge = parts[0];

                foreach (var node in parts[1].Split(','))
                {
                    yield return Tuple.Create(Edge.From(edge, isOutcoming), node);
                }
            }
        }
    }
}
