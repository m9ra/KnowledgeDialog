using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;

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
            var labelRepresentation = new Dictionary<string, object>();
            labelRepresentation["answer_mid"] = AnswerId;
            labelRepresentation["annotation"] = Annotation;
            labelRepresentation["has_correct_answer"] = Annotation == "correct_answer";

            var experiment_number_str = ExperimentId.Substring("question_collection_r_".Length);
            var experiment_number = int.Parse(experiment_number_str);

            var representation = new Dictionary<string, object>();
            representation["question"] = Question;
            representation["dialog_id"] = experiment_number + ExplanationTurns.First().Time.Ticks;
            representation["explanation_turns"] = convertTurns(ExplanationTurns, 2);
            representation["answer_turns"] = convertTurns(AnswerTurns, ExplanationTurns.Count() + 2);
            representation["label"] = labelRepresentation;
            representation["experiment_id"] = ExperimentId;

            return Newtonsoft.Json.JsonConvert.SerializeObject(representation);
        }

        private Dictionary<string, object>[] convertTurns(IEnumerable<AnnotatedQuestionActionEntry> turns, int turnOffset)
        {
            var sluFactory = new SLUFactory();
            var result = new List<Dictionary<string, object>>();
            var turnsArray = turns.ToArray();
            for (var i = 0; i < turnsArray.Length; i += 2)
            {
                var machineAction = turnsArray[i];
                var userAction = turnsArray[i + 1];

                var representation = new Dictionary<string, object>();
                result.Add(representation);

                representation["turn_index"] = (i + turnOffset) / 2;
                var output = new Dictionary<string, object>()
                {
                    {"time", machineAction.Time},
                    {"transcript",machineAction.Text},
                    {"dialog_acts", convertAct(machineAction.Act)}

                };

                var input = new Dictionary<string, object>()
                {
                    {"time", userAction.Time},
                    {"chat",userAction.Text},
                    {"chat_slu", getUserSlu(userAction,sluFactory)}
                };

                representation["output"] = output;
                representation["input"] = input;
            }

            return result.ToArray();
        }

        private Dictionary<string, object> getUserSlu(AnnotatedQuestionActionEntry userAction, SLUFactory sluFactory)
        {
            var slu = sluFactory.GetBestDialogAct(UtteranceParser.Parse(userAction.Text));
            if (slu is DontKnowAct || slu is NegateAct || slu is ChitChatAct || slu is AffirmAct)
                return slu.GetDialogAct().ToDictionary();

            return new Dictionary<string, object>()
            {
                {"act","Inform"},
                {"text",userAction.Text}
            };
        }

        private Dictionary<string, object> convertAct(string act)
        {
            //TODO this is ugly hack - because of json serialization failed during experiments..

            var result = new Dictionary<string, object>();

            var actName = act.Substring(0, act.IndexOf('('));
            result["act"] = actName;

            var parameters = act.Substring(actName.Length + 1, act.Length - actName.Length - 2);

            var atLeastFalse = "at_least=False";
            var atLeastTrue = "at_least=True";
            var isAtLeastFalse = parameters.Contains(atLeastFalse);
            var isAtLeastTrue = parameters.Contains(atLeastTrue);

            //we supose that two parameters will appear only if one of them is at least
            if (isAtLeastTrue || isAtLeastFalse)
            {
                result["at_least"] = isAtLeastTrue;
                if (parameters.Contains(",at_least="))
                {
                    atLeastTrue = "," + atLeastTrue;
                    atLeastFalse = "," + atLeastFalse;
                }
                parameters = parameters.Replace(atLeastTrue, "").Replace(atLeastFalse, "");
            }

            if (parameters == "")
                return result;

            var parameterName = parameters.Substring(0, parameters.IndexOf('='));
            var parameterValue = parameters.Substring(parameterName.Length + 1, parameters.Length - 1 - parameterName.Length);

            if (parameterValue.Contains('='))
                throw new NotImplementedException();

            result[parameterName] = parameterValue;
            return result;
        }
    }
}
