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

        internal bool IsUpdated;

        internal IEnumerable<string> Columns => _allColumns;

        /// <summary>
        /// How many times current criterions were used to read.
        /// </summary>
        internal int ReadCount { get; private set; }

        private readonly List<Dictionary<string, string>> _data = new List<Dictionary<string, string>>();

        private readonly List<Dictionary<string, string>> _result = new List<Dictionary<string, string>>();

        private readonly Dictionary<string, string> _criterions = new Dictionary<string, string>();

        private HashSet<string> _allColumns = new HashSet<string>();

        private string[] _currentColumns;

        internal DatabaseHandler()
        {
            refreshResult();
        }

        internal void SetCriterion(string column, string value)
        {
            ReadCount = 0;
            IsUpdated = true;
            _criterions[column] = value;

            refreshResult();
        }

        internal void ResetCriterions()
        {
            ReadCount = 0;
            IsUpdated = true;
            _criterions.Clear();
            refreshResult();
        }

        internal DatabaseHandler SetColumns(params string[] columns)
        {
            _currentColumns = columns.ToArray();
            _allColumns.UnionWith(_currentColumns);
            return this;
        }

        internal string GetSpecifier(string slotName)
        {
            _criterions.TryGetValue(slotName, out var value);
            return value;
        }

        internal string Read(string slot)
        {
            ReadCount += 1;
            if (!_result.Any())
                return null;

            return _result[0][slot];
        }

        internal DatabaseHandler Row(params string[] columnValues)
        {
            if (_currentColumns.Length != columnValues.Length)
                throw new InvalidOperationException("Invalid row insertion");

            var rowValue = new Dictionary<string, string>();
            for (var i = 0; i < _currentColumns.Length; ++i)
            {
                rowValue.Add(_currentColumns[i], columnValues[i]);
            }

            _data.Add(rowValue);
            return this;
        }

        internal IEnumerable<string> GetColumnValues(string column)
        {
            var domain = new HashSet<string>();
            foreach (var row in _data)
            {
                if (row.ContainsKey(column))
                    domain.Add(row[column]);
            }

            return domain;
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
