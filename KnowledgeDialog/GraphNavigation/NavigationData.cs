using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KnowledgeDialog.GraphNavigation
{
    [Serializable]
    public class NavigationData
    {
        private readonly object _L_data = new object();

        private Dictionary<string, EntityNavigationData> _entityData = new Dictionary<string, EntityNavigationData>();

        private Dictionary<string, EdgeNavigationData> _edgeData = new Dictionary<string, EdgeNavigationData>();

        public IEnumerable<EdgeNavigationData> EdgeData => _edgeData.Values;

        private readonly string _path;

        public NavigationData(string storagePath)
        {
            _path = storagePath;
            if (File.Exists(_path))
                load(_path);
        }

        public EntityNavigationData GetEntityData(string phrase)
        {
            lock (_L_data)
            {
                if (!_entityData.TryGetValue(phrase, out var data))
                    _entityData[phrase] = data = new EntityNavigationData(this, phrase);

                return data;
            }
        }

        internal void Save()
        {
            save(_path);
        }

        public EdgeNavigationData GetEdgeData(string edge)
        {
            lock (_L_data)
            {
                if (!_edgeData.TryGetValue(edge, out var data))
                    _edgeData[edge] = data = new EdgeNavigationData(this, edge);

                return data;
            }
        }

        private void save(string path)
        {
            lock (_L_data)
            {

                var serializer = new BinaryFormatter();
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var fields = new Dictionary<string, object>();
                    fields["_entityData"] = _entityData;
                    fields["_edgeData"] = _edgeData;
                    serializer.Serialize(stream, fields);
                    stream.Close();
                }
            }
        }

        private void load(string path)
        {
            lock (_L_data)
            {
                var formatter = new BinaryFormatter();
                Dictionary<string, object> fields;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fields = (Dictionary<string, object>)formatter.Deserialize(stream);
                    stream.Close();
                }

                _edgeData = (Dictionary<string, EdgeNavigationData>)fields["_edgeData"];
                _entityData = (Dictionary<string, EntityNavigationData>)fields["_entityData"];
            }
        }
    }
}
