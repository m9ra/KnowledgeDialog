using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebBackend.Dataset
{
    class AnnotatedDialogLogFile
    {
        private readonly LogFile _sourceFile;

        private readonly string PhraseAnnotationPath;

        private readonly Dictionary<int, string> _dialogAnnotations = new Dictionary<int, string>();

        internal string ExperimentId { get { return _sourceFile.ExperimentId; } }

        internal AnnotatedDialogLogFile(LogFile sourceFile)
        {
            _sourceFile = sourceFile;
            PhraseAnnotationPath = sourceFile.FilePath + ".pant";
            if (File.Exists(PhraseAnnotationPath))
            {
                var stringedData = File.ReadAllText(PhraseAnnotationPath);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(stringedData);

                _dialogAnnotations = (data["_dialogAnnotations"] as JObject).ToObject<Dictionary<int, string>>();
            }
            else
            {
                _dialogAnnotations = new Dictionary<int, string>();
            }
        }

        internal void SetAnnotation(AnnotatedQuestionActionEntry entry, string annotation)
        {
            if (annotation == null)
                _dialogAnnotations.Remove(entry.Entry.ActionIndex);
            else
                _dialogAnnotations[entry.Entry.ActionIndex] = annotation;
        }

        internal string GetAnnotation(AnnotatedQuestionActionEntry entry)
        {
            string annotation;
            _dialogAnnotations.TryGetValue(entry.Entry.ActionIndex, out annotation);
            return annotation;
        }

        internal void Save()
        {
            var data = new Dictionary<string, object>();
            data["_dialogAnnotations"] = _dialogAnnotations;
            var serializedData = JsonConvert.SerializeObject(data);

            var writer = new StreamWriter(PhraseAnnotationPath);
            writer.Write(serializedData);
            writer.Close();
        }

        internal IEnumerable<AnnotatedQuestionActionEntry> Annotate(IEnumerable<ActionEntry> actions)
        {
            var result = new List<AnnotatedQuestionActionEntry>();

            foreach (var action in actions)
            {
                string annotation;
                _dialogAnnotations.TryGetValue(action.ActionIndex, out annotation);

                result.Add(new AnnotatedQuestionActionEntry(action, annotation));
            }

            return result;
        }

        internal IEnumerable<AnnotatedQuestionActionEntry> LoadActions()
        {
            return Annotate(_sourceFile.LoadActions());
        }
    }
}
