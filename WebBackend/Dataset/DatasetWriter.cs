﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using WebBackend.Experiment;

namespace WebBackend.Dataset
{
    class DatasetWriter
    {
        /// <summary>
        /// Directory where source files are stored.
        /// </summary>
        internal readonly string SourceDir;

        internal DatasetWriter(ExperimentBase experiment)
        {
            SourceDir = experiment.ExperimentUserPath;
        }

        /// <summary>
        /// Writes dataset files into the target folder.
        /// </summary>
        /// <param name="targetDir">Target for dataset files.</param>
        internal void WriteData(string targetDir, SplitDescription trainSplit, SplitDescription validationSplit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Selects dialogs from compatible ones.
        /// </summary>
        /// <param name="compatibleDialogs">Dialogs compatible with split.</param>
        /// <param name="selectedDialogs">Dialogs selected for splits.</param>
        /// <param name="desiredDialogCount">How many dialogs is desired.</param>
        private IEnumerable<AnnotatedDialog> selectSplitDialogs(IEnumerable<AnnotatedDialog> compatibleDialogs, HashSet<AnnotatedDialog> selectedDialogs, int desiredDialogCount)
        {
            var splitDialogs = compatibleDialogs.Except(selectedDialogs).Take(desiredDialogCount).ToArray();
            selectedDialogs.UnionWith(splitDialogs);

            return splitDialogs;
        }


        /// <summary>
        /// Writes split file with given dialogs.
        /// </summary>
        /// <param name="targetDir">Target dir for the split file.</param>
        /// <param name="splitName">Name of the split file.</param>
        /// <param name="splitDialogs">Dialogs to write.</param>
        private void writeSplit(string targetDir, string splitName, IEnumerable<AnnotatedDialog> splitDialogs)
        {
            var trainTarget = File.CreateText(Path.Combine(targetDir, splitName + ".json"));
            foreach (var dialog in splitDialogs)
            {
                writeDialog(dialog, trainTarget);
            }
            trainTarget.Close();
        }

        /// <summary>
        /// Writes dialog with given writer.
        /// </summary>
        /// <param name="dialog">Dialog to write.</param>
        /// <param name="targetWriter">Target file for a dialog.</param>
        private void writeDialog(AnnotatedDialog dialog, TextWriter targetWriter)
        {
            var jsonDialog = dialog.ToJson();
            if (jsonDialog.Contains('\n'))
                throw new NotSupportedException("json cannot contain new lines");

            targetWriter.WriteLine(jsonDialog);
        }
    }
}
