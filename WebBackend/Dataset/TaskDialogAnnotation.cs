using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Dataset
{
    class TaskDialogAnnotation
    {
        public readonly TaskDialog Dialog;

        public string Annotation { get; private set; }

        private readonly string PhraseAnnotationPath;

        internal TaskDialogAnnotation(TaskDialog dialog)
        {
            var startTime = dialog.Start.Ticks;

            Dialog = dialog;
            PhraseAnnotationPath = dialog.LogFilePath + "." + startTime + ".dant";

            if (File.Exists(PhraseAnnotationPath))
            {
                Annotation = File.ReadAllText(PhraseAnnotationPath);
            }
        }

        internal void Annotate(string annotation)
        {
            Annotation = annotation;
            File.WriteAllText(PhraseAnnotationPath, Annotation);
        }
    }
}
