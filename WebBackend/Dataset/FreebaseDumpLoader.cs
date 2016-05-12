using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;

namespace WebBackend.Dataset
{
    class FreebaseDumpLoader
    {
        /// <summary>
        /// Path to the dump.
        /// </summary>
        private readonly string _dumpPath;

        /// <summary>
        /// Id to names.
        /// </summary>
        private readonly Dictionary<string, string[]> _names = new Dictionary<string, string[]>();

        /// <summary>
        /// Id to descriptions.
        /// </summary>
        private readonly Dictionary<string, string> _descriptions = new Dictionary<string, string>();

        internal IEnumerable<string> Ids { get { return _names.Keys; } }

        /// <summary>
        /// Path to the dump.
        /// </summary>
        /// <param name="dumpPath">Path to the dump.</param>
        internal FreebaseDumpLoader(string dumpPath)
        {
            _dumpPath = dumpPath;

            initialize();
        }

        private void initialize()
        {
            using (var fileStream = new FileStream(_dumpPath, FileMode.Open, FileAccess.Read))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var lineParts = line.Split(new[] { '\t' }, 4);

                    var freebaseId = lineParts[0];
                    var label = lineParts[1];
                    var aliases = lineParts[2].Substring(1).Trim();

                    if (label == null)
                        throw new NotImplementedException();

                    var description = lineParts[3];

                    _descriptions[freebaseId] = description;
                    var names = new List<string>();
                    names.Add(label);
                    if (aliases != "")
                    {
                        names.AddRange(aliases.Split(';'));
                    }
                    _names[freebaseId] = names.ToArray();

                }
            }
        }

        internal string GetLabel(string freebaseId)
        {
            var id = freebaseId.Substring(FreebaseLoader.IdPrefix.Length);

            string[] names;
            if (!_names.TryGetValue(id, out names) || names.Length <= 0)
                return null;

            return names[0];
        }

        internal string[] GetNames(string id)
        {
            string[] result;
            _names.TryGetValue(id, out result);

            return result;
        }

        internal string GetDescription(string id)
        {
            string result;
            _descriptions.TryGetValue(id, out result);

            return result;
        }
    }
}
