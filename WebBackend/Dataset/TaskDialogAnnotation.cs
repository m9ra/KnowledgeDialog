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
            Dialog = dialog;
            PhraseAnnotationPath = Path.Combine(Path.GetDirectoryName(dialog.LogFilePath), Dialog.Id + ".dant");

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
