using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using KnowledgeDialog.Dialog.Parsing;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    /// <summary>
    /// Provider which gives linked utterances.
    /// </summary>
    delegate LinkedUtterance LinkProvider(string utterance, IEnumerable<EntityInfo> context);

    class DiskCachedLinker : ILinker
    {
        private readonly LinkProvider _provider;

        private readonly int _version;

        private readonly string _cachePath;

        private Dictionary<string, LinkedUtterance> _cachedUtterances;

        internal bool CacheResult = true;

        internal DiskCachedLinker(string cachePath, int version, LinkProvider provider)
        {
            _cachePath = cachePath;
            _version = version;
            _provider = provider;
            loadCache();
        }

        public LinkedUtterance LinkUtterance(string utterance, IEnumerable<EntityInfo> context = null)
        {
            var key = getKey(utterance, context);

            LinkedUtterance result;
            if (!_cachedUtterances.TryGetValue(key, out result))
            {
                result = _provider(utterance, context);
                if (CacheResult)
                {
                    _cachedUtterances[key] = result;
                    cache();
                }
            }

            return result;
        }

        private string getKey(string utterance, IEnumerable<EntityInfo> context)
        {
            if (context == null)
                return utterance;

            var contextStr = string.Join(",", context.OrderBy(e => e.Mid).Select(e => FreebaseLoader.GetId(e.Mid)));
            return utterance + "|" + contextStr;
        }

        private void loadCache()
        {
            _cachedUtterances = new Dictionary<string, LinkedUtterance>();

            if (!File.Exists(_cachePath))
                return;

            var formatter = new BinaryFormatter();
            Dictionary<string, object> fields;
            using (var stream = new FileStream(_cachePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fields = (Dictionary<string, object>)formatter.Deserialize(stream);
                stream.Close();
            }

            var cachedVersion = (int)fields["version"];
            if (cachedVersion != _version)
                return;

            _cachedUtterances = (Dictionary<string, LinkedUtterance>)fields["utterances"];
        }

        private void cache()
        {
            var serializer = new BinaryFormatter();
            using (var stream = new FileStream(_cachePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var fields = new Dictionary<string, object>();
                fields["utterances"] = _cachedUtterances;
                fields["version"] = _version;
                serializer.Serialize(stream, fields);
                stream.Close();
            }
        }


    }
}
