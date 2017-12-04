using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class DatabaseHandler
    {
        internal int ResultCount => _result.Count;

        private readonly List<Dictionary<string, string>> _data = new List<Dictionary<string, string>>();

        private readonly List<Dictionary<string, string>> _result = new List<Dictionary<string, string>>();

        private readonly Dictionary<string, string> _criterions = new Dictionary<string, string>();

        internal DatabaseHandler()
        {
            refreshResult();
        }

        internal void SetCriterion(string category, string value)
        {
            _criterions[category] = value;

            refreshResult();
        }

        private void refreshResult()
        {
            _result.Clear();

            foreach (var item in _data)
            {
                if (isMatch(item))
                    _result.Add(item);
            }
        }

        private bool isMatch(Dictionary<string, string> item)
        {
            foreach (var criterion in _criterions)
            {
                if (!item.ContainsKey(criterion.Key))
                    return false;

                if (item[criterion.Key] != criterion.Value)
                    return false;
            }

            return true;
        }
    }
}
