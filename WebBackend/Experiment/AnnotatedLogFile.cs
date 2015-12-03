using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebBackend.Experiment
{
    class AnnotatedLogFile
    {
        /// <summary>
        /// Path for annotation file.
        /// </summary>
        internal readonly string AnnotationFilePath;

        /// <summary>
        /// File which is annotated.
        /// </summary>
        private readonly LogFile _sourceFile;

        /// <summary>
        /// Question answers indexed by ActionIndex.
        /// </summary>
        private Dictionary<int, string> _questionAnswers = new Dictionary<int, string>();

        internal AnnotatedLogFile(LogFile sourceFile)
        {
            _sourceFile = sourceFile;
            AnnotationFilePath = sourceFile.FilePath + ".ant";
            if (File.Exists(AnnotationFilePath))
            {
                var stringedData = File.ReadAllText(AnnotationFilePath);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(stringedData);

                _questionAnswers = (data["_questionAnswers"] as JObject).ToObject<Dictionary<int, string>>();
            }
        }


        internal void SetQuestionAnswers(Dictionary<int, string> correctAnswers)
        {
            _questionAnswers = new Dictionary<int, string>(correctAnswers);

            foreach (var key in correctAnswers.Keys)
            {
                if (_questionAnswers[key] == "")
                    //get rid off empty annotations
                    _questionAnswers.Remove(key);
            }
        }

        internal Dictionary<int, string> GetQuestionAnswers()
        {
            return new Dictionary<int, string>(_questionAnswers);
        }

        internal void Save()
        {
            var data = new Dictionary<string, object>();
            data["_questionAnswers"] = _questionAnswers;
            var serializedData = JsonConvert.SerializeObject(data);

            var writer = new StreamWriter(AnnotationFilePath);
            writer.Write(serializedData);
            writer.Close();
        }

        internal IEnumerable<AnnotatedActionEntry> Annotate(IEnumerable<ActionEntry> actions)
        {
            var result = new List<AnnotatedActionEntry>();

            foreach (var action in actions)
            {
                string correctAnswer;
                _questionAnswers.TryGetValue(action.ActionIndex, out correctAnswer);

                result.Add(new AnnotatedActionEntry(action, correctAnswer));
            }

            return result;
        }

        internal IEnumerable<AnnotatedActionEntry> LoadActions()
        {
            return Annotate(_sourceFile.LoadActions());
        }
    }
}
