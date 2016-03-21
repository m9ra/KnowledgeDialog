using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace WebBackend.Dataset
{
    class QuestionDatasetWriter
    {
        /// <summary>
        /// Path to target file of the dataset.
        /// </summary>
        internal readonly string TargetFile;

        /// <summary>
        /// 
        /// </summary>
        private readonly List<AnnotatedQuestionDialog> _dialogs = new List<AnnotatedQuestionDialog>();

        internal QuestionDatasetWriter(string file)
        {
            TargetFile = file;
        }

        internal void Add(AnnotatedQuestionDialog dialog)
        {
            _dialogs.Add(dialog);
        }

        internal void Save()
        {
            var writer = new StreamWriter(TargetFile);
            foreach (var dialog in _dialogs)
            {
                var jsonRepresentation = dialog.ToJson();
                if (jsonRepresentation.Contains('\n'))
                    throw new NotSupportedException("json cannot contain new lines");

                writer.WriteLine(jsonRepresentation);
            }

            writer.Close();
        }
    }
}
