using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WebBackend.Dataset
{
    class AnnotatedQuestionDialog
    {
        private readonly AnnotatedQuestionLogFile _log;

        public DateTime DialogEnd { get { return AnswerTurns.Last().Entry.Time; } }

        public string ExperimentId { get { return _log.ExperimentId; } }

        public readonly string Question;

        public readonly string AnswerId;

        public string AnswerLink { get { return "http://" + AnswerId; } }

        public string Annotation { get; private set; }

        public readonly AnnotatedQuestionActionEntry OpeningTurn;

        public readonly IEnumerable<string> AnswerNames;

        public readonly IEnumerable<AnnotatedQuestionActionEntry> ExplanationTurns;

        public readonly IEnumerable<AnnotatedQuestionActionEntry> AnswerTurns;

        internal AnnotatedQuestionDialog(AnnotatedQuestionLogFile log, string question, string answerId, IEnumerable<string> answerNames, IEnumerable<AnnotatedQuestionActionEntry> explanationTurns, IEnumerable<AnnotatedQuestionActionEntry> answerTurns)
        {
            _log = log;
            Question = question;
            AnswerId = answerId;
            AnswerNames = answerNames;

            ExplanationTurns = explanationTurns.ToArray();
            AnswerTurns = answerTurns.ToArray();

            OpeningTurn = ExplanationTurns.First();
            Annotation = _log.GetAnnotation(OpeningTurn);
        }

        internal void Annotate(string annotation)
        {
            if (annotation == "none")
                annotation = null;

            _log.SetAnnotation(OpeningTurn, annotation);
            _log.Save();

            Annotation = annotation;
        }

        internal string ToJson()
        {
            var labelRepresentation = new Dictionary<string, string>();
            labelRepresentation["answer_mid"] = AnswerId;
            labelRepresentation["annotation"] = Annotation;

            var representation = new Dictionary<string, object>();
            representation["question"] = Question;
            representation["explanation_turns"] = convertTurns(ExplanationTurns);
            representation["answer_turns"] = convertTurns(AnswerTurns);
            representation["label"] = labelRepresentation;
            representation["experiment_id"] = ExperimentId;

            return Newtonsoft.Json.JsonConvert.SerializeObject(representation);
        }

        private Dictionary<string, object>[] convertTurns(IEnumerable<AnnotatedQuestionActionEntry> turns)
        {
            var result = new List<Dictionary<string, object>>();
            var turnsArray = turns.ToArray();
            for (var i = 0; i < turnsArray.Length; i += 2)
            {
                var machineAction = turnsArray[i];
                var userAction = turnsArray[i + 1];

                var representation=new Dictionary<string,object>();
                result.Add(representation);

                representation["turn_id"] = i / 2;
                representation["machine_action_text"] = machineAction.Entry.Text;
                throw new NotImplementedException("make this same as in DSTC");
            }

            return result.ToArray();
        }
    }
}
