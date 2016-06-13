using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.IO;

namespace WebBackend
{
    static class ComputationCache
    {
        internal readonly static string CachePath = "./cache";

        internal static CachedData Load<CachedData>(string dataSource, int version, Func<CachedData> factory)
        {
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);

            var cachePath = getCachePath(dataSource, version);

            var entry = loadData(cachePath);
            if (entry == null || entry.DataSource != dataSource || entry.Version != version)
            {
                var data = factory();
                saveData(cachePath, dataSource, version, data);
            }

            return (CachedData)loadData(cachePath).Data;
        }

        private static CacheEntry loadData(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                var formatter = new BinaryFormatter();
                using (var filestream = new FileStream(path, FileMode.Open))
                {
                    var entry = formatter.Deserialize(filestream) as CacheEntry;
                    return entry;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private static void saveData<CachedData>(string path, string dataSource, int version, CachedData data)
        {
            var entry = new CacheEntry(dataSource, version, data);
            var formatter = new BinaryFormatter();
            using (var filestream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(filestream, entry);
            }
        }

        private static string getCachePath(string dataSource, int version)
        {
            return CachePath + "/" + dataSource + ".bin";
        }
    }

    [Serializable]
    class CacheEntry
    {
        internal readonly string DataSource;

        internal readonly int Version;

        internal readonly object Data;

        internal CacheEntry(string dataSource, int version, object data)
        {
            DataSource = dataSource;
            Version = version;
            Data = data;
        }
    }
}
