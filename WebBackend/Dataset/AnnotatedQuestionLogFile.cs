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
    class AnnotatedQuestionLogFile
    {
        private readonly LogFile _sourceFile;

        private readonly string QuestionAnnotationPath;

        private readonly Dictionary<int, string> _questionAnnotations = new Dictionary<int, string>();

        internal string ExperimentId { get { return _sourceFile.ExperimentId; } }

        internal AnnotatedQuestionLogFile(LogFile sourceFile)
        {
            _sourceFile = sourceFile;
            QuestionAnnotationPath = sourceFile.FilePath + ".qant";
            if (File.Exists(QuestionAnnotationPath))
            {
                var stringedData = File.ReadAllText(QuestionAnnotationPath);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(stringedData);

                _questionAnnotations = (data["_questionAnnotations"] as JObject).ToObject<Dictionary<int, string>>();
            }
            else
            {
                _questionAnnotations = new Dictionary<int, string>();
            }
        }

        internal void SetAnnotation(AnnotatedQuestionActionEntry entry, string annotation)
        {
            if (annotation == null)
                _questionAnnotations.Remove(entry.Entry.ActionIndex);
            else
                _questionAnnotations[entry.Entry.ActionIndex] = annotation;
        }

        internal string GetAnnotation(AnnotatedQuestionActionEntry entry)
        {
            string annotation;
            _questionAnnotations.TryGetValue(entry.Entry.ActionIndex, out annotation);
            return annotation;
        }

        internal void Save()
        {
            var data = new Dictionary<string, object>();
            data["_questionAnnotations"] = _questionAnnotations;
            var serializedData = JsonConvert.SerializeObject(data);

            var writer = new StreamWriter(QuestionAnnotationPath);
            writer.Write(serializedData);
            writer.Close();
        }

        internal IEnumerable<AnnotatedQuestionActionEntry> Annotate(IEnumerable<ActionEntry> actions)
        {
            var result = new List<AnnotatedQuestionActionEntry>();

            foreach (var action in actions)
            {
                string annotation;
                _questionAnnotations.TryGetValue(action.ActionIndex, out annotation);

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
