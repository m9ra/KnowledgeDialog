using System;
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
        internal void WriteData(string targetDir)
        {
            var target = File.CreateText(Path.Combine(targetDir, "TODO_split.json"));
            foreach (var logFile in LogFile.Load(SourceDir))
            {
                var annotatedLogFile = new AnnotatedLogFile(logFile);
                foreach (var dialog in AnnotatedDialogBuilder.ParseDialogs(annotatedLogFile))
                {
                    writeDialog(dialog, target);
                }
            }
            target.Close();
            throw new NotImplementedException("Create splits");
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
            throw new NotImplementedException();
        }
    }
}
