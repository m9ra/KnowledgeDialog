using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;


namespace WebBackend
{
    public class ActionEntry
    {
        public readonly DateTime Time;

        public readonly string Text;

        public readonly string Type;

        public readonly string UserId;

        public readonly int TaskId = -1;

        public readonly string Act;

        public readonly string MachineActJson;

        public readonly int ActionIndex;

        public bool IsReset { get { return Type == "T_Info" && Text.Trim().Contains("new console"); } }

        internal readonly Dictionary<string, object> Data;


        internal bool IsRegularTurn
        {
            get
            {
                return
                    Type == "T_utterance" ||
                    Type == "T_response"
                    ;
            }
        }

        public bool IsCompletition
        {
            get
            {
                return Type == "T_completition";
            }
        }

        public bool IsInfo
        {
            get
            {
                return Type == "T_info";
            }
        }

        public bool IsDialogStart
        {
            get
            {
                var isOpeningAct =
                    //(Type == "T_Info" && Text == "new console ") ||
                    (Type == "T_utterance" && Text == "Reset") ||
                    Type == "T_task" ||
                    Type == "T_reset"
                ;

                return isOpeningAct;
            }
        }

        private static readonly SLUFactory _factory = new SLUFactory();


        internal bool HasUserId()
        {
            return UserId != null;
        }

        internal ActionEntry(int actionIndex, Dictionary<string, object> data)
        {
            ActionIndex = actionIndex;
            Data = data;

            if (data.ContainsKey(CallStorage.TimeEntry))
                Time = (DateTime)data[CallStorage.TimeEntry];

            if (data.ContainsKey("user_id"))
                UserId = data["user_id"] as string;

            if (data.ContainsKey("task_id"))
                TaskId = int.Parse(data["task_id"] as string);

            Type = "T_" + resolveType(data);

            object textData;
            if (
                data.TryGetValue("message", out textData) ||
                data.TryGetValue("utterance", out textData) ||
                data.TryGetValue("response", out textData)
                )
                Text = textData as string;
            else
                Text = "";

            object actData;
            if (data.TryGetValue("response_act", out actData))
            {
                if (actData != null)
                    Act = actData.ToString();
            }

            if (data.TryGetValue("response_act_json", out actData))
            {
                if (actData != null)
                    MachineActJson = actData.ToString();
            }

            if (data.ContainsKey("task"))
            {
                var substitutions = data["substitutions"] as Newtonsoft.Json.Linq.JArray;
                Text = string.Format(data["format"].ToString(), substitutions.ToArray());

                if (data.ContainsKey("success_code"))
                {
                    Text = $"Success: {data["success_code"]}, " + Text;
                }
            }

            switch (Type)
            {
                case "T_advice":
                    Text = "<b>Question</b>:" + data["question"] + "<br>";
                    Text += "<b>Context</b>:" + data["context"] + "<br>";
                    Text += "<b>Answer</b>:" + data["correctAnswerNode"] + "<br><br>";
                    break;

                case "T_equivalence":
                    Text = "<b>PatternQuestion</b>:" + data["patternQuestion"] + "<br>";
                    Text += "<b>QueriedQuestion</b>:" + data["queriedQuestion"] + "<br>";
                    Text += "<b>IsEquivalent</b>:" + data["isEquivalent"] + "<br><br>";
                    break;

                case "T_utterance":
                    var utterance = UtteranceParser.Parse(data["utterance"] as string);
                    Act = _factory.GetBestDialogAct(utterance).ToString();
                    break;
            }
        }

        internal string ParseAct(string start, string end)
        {
            if (Act == null)
                return null;

            var startIndex = Act.IndexOf(start);
            if (startIndex < 0)
                return null;

            startIndex += start.Length;

            var endIndex = Act.IndexOf(end, startIndex);
            if (endIndex < 0)
                return null;

            return Act.Substring(startIndex, endIndex - startIndex);
        }

        private string resolveType(Dictionary<string, object> data)
        {
            var callName = data[CallStorage.CallNameEntry] as string;
            switch (callName)
            {
                case "ReportTaskCompletition":
                    return "completition";
                case "AdviceAnswer":
                    return "advice";
                case "SetEquivalence":
                    return "equivalence";
            }

            if (data.ContainsKey("task"))
                return "task";

            if (data.ContainsKey("utterance") && data["utterance"].ToString().ToLowerInvariant().Trim() == "reset")
                return "reset";

            if (data.ContainsKey("utterance"))
                return "utterance";

            if (data.ContainsKey("response"))
                return "response";

            return callName;
        }

        public override string ToString()
        {
            return $"{Type}: {Text}";
        }
    }
}
