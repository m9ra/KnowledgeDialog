using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.DataCollection;

using WebBackend.Experiment;

namespace WebBackend.Dataset
{
    class QuestionDialogProvider
    {
        private readonly List<ExperimentBase> _experiments = new List<ExperimentBase>();

        private readonly List<AnnotatedQuestionDialog> _questionDialogs = new List<AnnotatedQuestionDialog>();

        private readonly QuestionCollection _questions;

        internal int DialogCount { get { return _questionDialogs.Count; } }

        internal QuestionDialogProvider(ExperimentCollection collection, QuestionCollection questions, string experimentPrefix)
        {
            _questions = questions;

            foreach (var experiment in collection.Experiments)
            {
                if (experiment.Id.StartsWith(experimentPrefix))
                    _experiments.Add(experiment);
            }

            _experiments.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        internal void Refresh()
        {
            _questionDialogs.Clear();
            var logFiles = new List<LogFile>();
            foreach (var experiment in _experiments)
            {
                foreach (var logFile in experiment.LoadLogFiles())
                {
                    logFiles.Add(logFile);
                }
            }

            foreach (var logFile in logFiles)
            {
                var annotatedLogFile = new AnnotatedQuestionLogFile(logFile);
                foreach (var dialog in AnnotatedQuestionDialogBuilder.ParseDialogs(annotatedLogFile, _questions))
                {
                    _questionDialogs.Add(dialog);
                }
            }

            _questionDialogs.Sort((a, b) => a.DialogEnd.CompareTo(b.DialogEnd));
        }

        internal AnnotatedQuestionDialog GetDialog(int dialogIndex)
        {
            if (dialogIndex >= _questionDialogs.Count)
                return null;

            return _questionDialogs[dialogIndex];
        }
    }
}
