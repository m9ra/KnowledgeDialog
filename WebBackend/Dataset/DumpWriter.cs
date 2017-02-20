using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;

namespace WebBackend.Dataset
{
    class DumpWriter
    {
        /// <summary>
        /// Writter of the resulting dump.
        /// </summary>
        private readonly StreamWriter _writer;

        internal DumpWriter(string dumpFile)
        {
            var fileStream = new FileStream(dumpFile, FileMode.Create, FileAccess.Write);
            _writer = new StreamWriter(fileStream);
        }

        internal void Write(string freebaseId, string label, IEnumerable<string> aliases, string description, IEnumerable<Tuple<string, string>> inEdges, IEnumerable<Tuple<string, string>> outEdges)
        {
            var aliasesStr = sanitizeAlias(label);
            if (aliases.Count() > 0)
                aliasesStr += ";" + string.Join(";", aliases.Select(a => sanitizeAlias(a)));

            var inEdgesStr = formatEdges(inEdges);
            var outEdgesStr = formatEdges(outEdges);

            var outputLine = freebaseId + "\t" + aliasesStr + "\t" + inEdgesStr + "\t" + outEdgesStr + "\t" + description;
            _writer.Write(outputLine.Replace('\n', ' ') + "\n");
        }

        private string sanitizeAlias(string alias)
        {
            return alias.Replace('\t', ' ').Replace(';', ',');
        }

        private string formatEdges(IEnumerable<Tuple<string, string>> edges)
        {
            if (edges == null)
                return "";

            var edgeIndex = new Dictionary<string, List<string>>();
            foreach (var edge in edges)
            {
                List<string> nodes;
                if (!edgeIndex.TryGetValue(edge.Item1, out nodes))
                    edgeIndex[edge.Item1] = nodes = new List<string>();

                nodes.Add(edge.Item2);
            }

            var edgeAssignments = new List<string>();
            foreach (var index in edgeIndex)
            {
                var edgeAssignment = index.Key + ":" + string.Join(",", index.Value);
                edgeAssignments.Add(edgeAssignment);
            }

            return string.Join(";", edgeAssignments);
        }

        internal void Close()
        {
            _writer.Close();
        }
    }
}
