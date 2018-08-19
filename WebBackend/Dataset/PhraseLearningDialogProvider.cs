using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBackend.Experiment;

namespace WebBackend.Dataset
{
    class PhraseLearningDialogProvider
    {
        private readonly List<ExperimentBase> _experiments = new List<ExperimentBase>();

        private readonly List<TaskDialogAnnotation> _dialogAnnotations = new List<TaskDialogAnnotation>();

        internal int DialogCount { get { return _dialogAnnotations.Count; } }

        internal PhraseLearningDialogProvider(ExperimentCollection collection, params string[] experimentsIds)
        {
            foreach (var experiment in collection.Experiments)
            {
                if (experimentsIds.Contains(experiment.Id))
                    _experiments.Add(experiment);
            }

            _experiments.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        internal TaskDialogAnnotation GetDialog(int dialogIndex)
        {
            if (dialogIndex >= _dialogAnnotations.Count || dialogIndex < 0)
                return null;

            return _dialogAnnotations[dialogIndex];
        }

        internal void Refresh()
        {
            _dialogAnnotations.Clear();
            var logFiles = new List<LogFile>();
            foreach (var experiment in _experiments)
            {
                foreach (var logFile in experiment.LoadLogFiles())
                {
                    foreach (var dialog in logFile.ParseDialogs())
                    {
                        var annotatedDialog = new TaskDialogAnnotation(dialog);
                        _dialogAnnotations.Add(annotatedDialog);
                    }
                }
            }

            _dialogAnnotations.Sort((a, b) => a.Dialog.Start.CompareTo(b.Dialog.Start));
        }
    }
}
