﻿using System;
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

        private string[] _currentColumns;

        internal DatabaseHandler()
        {
            refreshResult();
        }

        internal void SetCriterion(string category, string value)
        {
            _criterions[category] = value;

            refreshResult();
        }

        internal DatabaseHandler Columns(params string[] columns)
        {
            _currentColumns = columns.ToArray();
            return this;
        }


        internal string Read(string slot)
        {
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
