using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Newtonsoft.Json;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.Database
{
    public delegate void StorageCallback(StorageReader reader);

    public class CallStorage
    {
        public static readonly string TimeEntry = "_time";

        public static readonly string CallNameEntry = "_callname";

        private readonly Dictionary<string, CallSerializer> _calls = new Dictionary<string, CallSerializer>();

        private readonly string _file;

        private StreamWriter _writer;

        private bool _supressOutput;

        private bool _wasRead = false;

        private readonly JsonSerializerSettings _serializationSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None
        };

        public CallStorage(string file)
        {
            _file = file;
        }

        public void ReadStorage()
        {
            if (_file == null || !File.Exists(_file) || _wasRead)
                return;

            _wasRead = true;
            _supressOutput = true;

            try
            {
                var fs = new FileStream(_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                using (var reader = new StreamReader(fs))
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine().Trim();
                        if (line == "")
                            continue;

                        var storage = JsonConvert.DeserializeObject<Dictionary<string, object>>(line);

                        var callName = storage[CallNameEntry] as string;
                        var serializer = _calls[callName];

                        serializer.RecallWith(storage);
                    }
            }
            finally
            {
                _supressOutput = false;
            }
        }

        public CallSerializer RegisterCall(string callName, StorageCallback callback)
        {
            var call = new CallSerializer(this, callName, callback);
            _calls[callName] = call;

            return call;
        }

        internal void Save(Dictionary<string, object> storage)
        {
            if (_file == null || _supressOutput)
                //serialization is disabled
                return;

            if (_writer == null)
            {
                var fs = new FileStream(_file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                _writer = new StreamWriter(fs);
            }

            var json = JsonConvert.SerializeObject(storage, _serializationSettings);
            _writer.WriteLine(json);
            _writer.Flush();
        }

        internal void Close()
        {
            if (_writer != null)
                _writer.Close();
        }
    }

    public class CallSerializer
    {
        private readonly string _callName;

        private readonly StorageCallback _callback;

        private readonly Dictionary<string, object> _storage = new Dictionary<string, object>();

        private readonly CallStorage _owner;

        internal CallSerializer(CallStorage owner, string callName, StorageCallback callback)
        {
            _callName = callName;
            _callback = callback;
            _owner = owner;
        }

        public void ReportParameter(string name, string value)
        {
            _storage.Add(name, value);
        }

        public void ReportParameter(string name, bool value)
        {
            _storage.Add(name, value);
        }

        public void ReportParameter(string name, NodeReference node)
        {
            var value = node.Data;
            _storage.Add(name, value);
        }

        public void ReportParameter(string name, IEnumerable<NodeReference> nodes)
        {
            var data = new List<object>();
            foreach (var node in nodes)
            {
                data.Add(node.Data);
            }

            _storage.Add(name, data.ToArray());
        }

        public void SaveReport()
        {
            _storage.Add(CallStorage.CallNameEntry, _callName);
            _storage.Add(CallStorage.TimeEntry, DateTime.Now);
            _owner.Save(_storage);
            _storage.Clear();
        }

        internal void RecallWith(Dictionary<string, object> storage)
        {
            _callback(new StorageReader(storage));
        }
    }

    public class StorageReader
    {
        private readonly Dictionary<string, object> _storage;

        public StorageReader(Dictionary<string, object> storage)
        {
            _storage = storage;
        }

        public string String(string p)
        {
            return _storage[p] as string;
        }

        public bool Bool(string p)
        {
            return (bool)_storage[p];
        }


        public NodeReference Node(string p, ComposedGraph graph)
        {
            //TODO not only strings can be present as node data
            return graph.GetNode(_storage[p].ToString());
        }

        public IEnumerable<NodeReference> Nodes(string p, ComposedGraph graph)
        {
            var result = new List<NodeReference>();
            var data = _storage[p] as IEnumerable<object>;

            foreach (var nodeData in data)
            {
                //TODO not only strings can be present as node data
                var stringData = nodeData.ToString();
                result.Add(graph.GetNode(stringData));
            }

            return result;
        }
    }
}
