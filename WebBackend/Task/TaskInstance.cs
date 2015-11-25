using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace WebBackend
{
    class TaskInstance
    {
        public readonly string TaskFormat;

        public readonly IEnumerable<NodeReference> Substitutions;

        public IEnumerable<NodeReference> ExpectedAnswers { get { return _expectedAnswers; } }

        public virtual bool IsComplete { get { return _turns > 3 && _containsAnswer; } }

        public string Text { get { return string.Format(TaskFormat, Substitutions.Select(s => "'" + s.Data + "'").ToArray()); } }

        internal readonly int ValidationCodeKey;

        internal readonly int ValidationCode;

        internal readonly string Key;

        private readonly NodeReference[] _expectedAnswers;

        private bool _containsAnswer;

        private int _turns = 0;

        public TaskInstance(string taskFormat, IEnumerable<NodeReference> substitutions, IEnumerable<NodeReference> expectedAnswers, string key, int validationCodeKey)
        {
            TaskFormat = taskFormat;
            Substitutions = substitutions.ToArray();
            _expectedAnswers = expectedAnswers.ToArray();
            ValidationCodeKey = validationCodeKey;
            ValidationCode = validationCodeKey * 65479 % 10000;
            Key = key;

            if (_expectedAnswers.Length == 0)
                //we cannot find correct answer
                _containsAnswer = true;
        }

        internal virtual void Register(string utterance, ResponseBase response)
        {
            if (response == null)
                return;

            ++_turns;

            if (_containsAnswer)
                //no more we need checking for answer presence.
                return;

            var str = response.ToString();
            foreach (var expectedAnswer in _expectedAnswers)
            {
                if (str.ToLowerInvariant().Contains(expectedAnswer.Data.ToString().ToLowerInvariant()))
                    _containsAnswer = true;
            }
        }
    }
}
