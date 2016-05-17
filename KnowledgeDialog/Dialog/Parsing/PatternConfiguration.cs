using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Parsing
{
    class PatternConfiguration
    {
        private Dictionary<string, string[]> _keyToGroup = new Dictionary<string, string[]>();

        internal void RegisterGroup(string key, string[] group)
        {
            _keyToGroup[key] = group;
        }

        internal IEnumerable<string> GetGroup(string key)
        {
            return _keyToGroup[key];
        }
    }
}
