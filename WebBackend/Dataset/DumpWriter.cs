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
            var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
            _writer = new StreamWriter(gzipStream);
        }

        internal void Write(string freebaseId, string label, IEnumerable<string> aliases, string description, int inBounds, int outBounds)
        {
            var outputLine = freebaseId + "\t" + inBounds + "\t" + outBounds + "\t" + label + "\t;" + string.Join(";", aliases) + "\t" + description;
            _writer.WriteLine(outputLine);
        }

        internal void Close()
        {
            _writer.Close();
        }
    }
}
