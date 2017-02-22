using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;
using KnowledgeDialog.Knowledge;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{

    class ExtractionKnowledge
    {
        private static readonly List<ExtractionKnowledge> _registeredKnowledge = new List<ExtractionKnowledge>();

        private readonly object _L_global = new object();

        private Dictionary<string, QuestionInfo> _questionIndex = new Dictionary<string, QuestionInfo>();

        private Random _rnd = new Random();

        internal readonly string StoragePath;

        internal static IEnumerable<ExtractionKnowledge> RegisteredKnowledge { get { return _registeredKnowledge; } }

        internal int QuestionCount { get { return _questionIndex.Count; } }

        internal IEnumerable<QuestionInfo> Questions { get { return _questionIndex.Values; } }

        internal ExtractionKnowledge(string storage)
        {
            StoragePath = storage;

            if (StoragePath != null)
                deserializeFrom(StoragePath);

            _registeredKnowledge.Add(this);
        }

        internal QuestionInfo GetInfo(string question)
        {
            lock (_L_global)
            {
                QuestionInfo result;
                _questionIndex.TryGetValue(question, out result);

                return result;
            }
        }

        internal void AddQuestion(string question)
        {
            lock (_L_global)
            {
                if (_questionIndex.ContainsKey(question))
                    return;

                _questionIndex[question] = new QuestionInfo(UtteranceParser.Parse(question));

                commitChanges();
            }
        }

        internal void AddAnswerHint(QuestionInfo questionInfo, ParsedUtterance utterance)
        {
            lock (_L_global)
            {
                //TODO thread safe
                var newInfo = questionInfo.WithAnswerHint(utterance);
                _questionIndex[questionInfo.Utterance.OriginalSentence] = newInfo;

                commitChanges();
            }
        }

        internal string GetRandomQuestion()
        {
            lock (_L_global)
            {
                var questions = _questionIndex.Keys.ToArray();

                var randomQIndex = _rnd.Next(questions.Length);
                return questions[randomQIndex];
            }
        }

        private void serializeTo(string path)
        {
            var config = new Dictionary<string, object>();

            config["_questionIndex"] = _questionIndex;
            config["_rnd"] = _rnd;

            lock (_L_global)
            {
                using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(file, config);
                }
            }
        }

        private void deserializeFrom(string path)
        {
            lock (_L_global)
            {
                if (!File.Exists(path))
                    return;

                using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    var formatter = new BinaryFormatter();
                    var config = (Dictionary<string, object>)formatter.Deserialize(file);

                    _questionIndex = (Dictionary<string, QuestionInfo>)config["_questionIndex"];
                    _rnd = (Random)config["_rnd"];
                }
            }
        }

        private void commitChanges()
        {
            if (StoragePath == null)
                //no storage defined
                return;

            serializeTo(StoragePath);
        }
    }
}
