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

        public bool IsComplete { get { return _turns > 3 && _containsAnswer; } }

        public bool CompletitionReported { get; private set; }

        public string Text { get { return string.Format(TaskFormat, Substitutions.Select(s => "<b>" + s.Data + "</b>").ToArray()); } }


        private readonly NodeReference[] _expectedAnswers;

        private readonly UserTracker _user;

        private readonly string _key;

        private bool _containsAnswer;

        private int _turns = 0;

        public TaskInstance(string taskFormat, IEnumerable<NodeReference> substitutions, IEnumerable<NodeReference> expectedAnswers, string key, UserTracker user)
        {
            TaskFormat = taskFormat;
            Substitutions = substitutions.ToArray();
            _expectedAnswers = expectedAnswers.ToArray();
            _user = user;
            _key = key;

            if (_expectedAnswers.Length == 0)
                //we cannot find correct answer
                _containsAnswer = true;
        }

        internal void Register(ResponseBase response)
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

        internal void ReportCompletition()
        {
            CompletitionReported = true;
            _user.ReportTaskCompletition(_key, TaskFormat, Substitutions);
        }

        internal void ReportStart()
        {
            _user.ReportTaskStart(_key, TaskFormat, Substitutions);
        }
    }
}
