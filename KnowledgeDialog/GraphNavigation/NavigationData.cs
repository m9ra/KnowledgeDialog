using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.GraphNavigation
{
    public class NavigationData
    {
        private readonly object _L_data = new object();

        private readonly Dictionary<string, EntityNavigationData> _entityData = new Dictionary<string, EntityNavigationData>();

        private readonly string _path;

        public NavigationData(string storagePath)
        {
            _path = storagePath;
        }

        public EntityNavigationData GetData(string phrase)
        {
            lock (_L_data)
            {
                if (!_entityData.TryGetValue(phrase, out var data))
                    _entityData[phrase] = data = new EntityNavigationData(this, phrase);

                return data;
            }
        }

        private void save(string path)
        {
            lock (_L_data)
            {
                throw new NotImplementedException();
            }
        }

        private  void load(string path)
        {
            lock (_L_data)
            {
                throw new NotImplementedException();
            }
        }

    }
}
