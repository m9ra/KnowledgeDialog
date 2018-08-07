using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KnowledgeDialog.Knowledge
{
    [Serializable]
    public class VoteContainer<T>
    {
        private Dictionary<T, Votes> _itemsVotes = new Dictionary<T, Votes>();

        private readonly string _serializedPath;

        private readonly object _L_container = new object();

        public IEnumerable<KeyValuePair<T, Votes>> ItemsVotes => _itemsVotes.ToArray();

        public IEnumerable<KeyValuePair<T, Votes>> SortedItemsVotes => ItemsVotes.OrderByDescending(p => p.Value.Total).ToArray();

        public VoteContainer(string path = null)
        {
            _serializedPath = path;
            load();
        }

        public void Vote(T item)
        {
            lock (_L_container)
            {
                if (!_itemsVotes.TryGetValue(item, out var votes))
                    _itemsVotes[item] = votes = new Votes();

                votes.AddPositiveVotes(1);
                save();
            }
        }

        private void save()
        {
            if (_serializedPath == null)
                return;

            lock (_L_container)
            {
                var fs = new FileStream(_serializedPath, FileMode.Create);
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, _itemsVotes);
                fs.Close();
            }
        }

        private void load()
        {
            if (_serializedPath == null)
                return;

            lock (_L_container)
            {
                if (!File.Exists(_serializedPath))
                    return;

                var serializer = new BinaryFormatter();
                var stream = new FileStream(_serializedPath, FileMode.Open);
                _itemsVotes = (Dictionary<T, Votes>)serializer.Deserialize(stream);
            }
        }
    }
}
