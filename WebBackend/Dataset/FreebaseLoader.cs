using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;

namespace WebBackend.Dataset
{
    class FreebaseLoader
    {
        internal static readonly string EdgePrefix = "www.freebase.com";

        internal static readonly string IdPrefix = "www.freebase.com/m/";

        internal static string EnglishSuffix = "@en";

        internal static readonly string ApiUrlPrefix = "https://www.googleapis.com/freebase/v1/rdf/m/";

        internal static readonly string ApiKey = "AIzaSyBq2ESLNAtgJT9zXUVoDG3vUlnRIsL1tKM";

        internal readonly string CachePath;

        private readonly Dictionary<string, IEnumerable<string>> _idToNames = new Dictionary<string, IEnumerable<string>>();

        private readonly Dictionary<string, string> _idToDescription = new Dictionary<string, string>();

        internal FreebaseLoader(string cachePath)
        {
            CachePath = cachePath;
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
        }

        internal IEnumerable<string> GetCachedIds()
        {
            foreach (var file in Directory.EnumerateFiles(CachePath, "*.sdx"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                yield return IdPrefix + name;
            }
        }

        internal IEnumerable<string> GetNames(string answerId)
        {
            if (!answerId.StartsWith(IdPrefix))
                throw new NotSupportedException("unsupported answer");

            var id = answerId.Substring(IdPrefix.Length);

            IEnumerable<string> names;
            if (!_idToNames.TryGetValue(id, out names))
            {
                var lines = new List<string>();
                lines.AddRange(filterFileLines(id, "ns:type.object.name", "@en;"));
                lines.AddRange(filterFileLines(id, "ns:common.topic.alias", "@en;"));
                _idToNames[id] = names = lines;
            }

            return names;
        }

        internal string GetDescription(string answerId)
        {
            if (!answerId.StartsWith(IdPrefix))
                throw new NotSupportedException("unsupported answer");

            var id = answerId.Substring(IdPrefix.Length);
            string description;
            if (!_idToDescription.TryGetValue(id, out description))
            {
                var lines = new List<string>();
                lines.AddRange(filterFileLines(id, "ns:common.topic.description", "@en;"));
                if (lines.Count == 0)
                    description = "";
                else
                    description = lines.First();

                _idToDescription[id] = description;
            }

            return description;
        }

        private IEnumerable<string> filterFileLines(string id, string prefix, string suffix)
        {
            var fileLines = getFileLines(id);
            var result = new List<string>();
            foreach (var line in fileLines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(prefix) && trimmedLine.EndsWith(suffix))
                {
                    var resultLine = trimmedLine.Substring(prefix.Length, trimmedLine.Length - prefix.Length - suffix.Length).Trim();
                    result.Add(resultLine);
                }
            }
            return result;
        }

        private IEnumerable<string> getFileLines(string id)
        {
            var filePath = Path.Combine(CachePath, id + ".sdx");
            if (!File.Exists(filePath))
                return new string[0];
           //     downloadFile(ApiUrlPrefix + id + "?key=" + ApiKey, filePath);

            return File.ReadAllLines(filePath);
        }

        private void downloadFile(string url, string targetPath)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, targetPath);
            }
        }
    }
}
