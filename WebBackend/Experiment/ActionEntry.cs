﻿using System;
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

        public readonly DialogActBase Act;

        internal readonly Dictionary<string, object> Data;

        private static readonly SLUFactory _factory = new SLUFactory();

        internal bool HasUserId()
        {
            return UserId != null;
        }

        internal ActionEntry(Dictionary<string, object> data)
        {
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

            if (data.ContainsKey("task"))
            {
                var substitutions = data["substitutions"] as Newtonsoft.Json.Linq.JArray;
                Text = string.Format(data["format"].ToString(), substitutions.ToArray());
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
                    Act = _factory.GetDialogAct(utterance);
                    break;
            }
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

            if (data.ContainsKey("utterance") && data["utterance"].ToString().Trim() == "reset")
                return "reset";

            if (data.ContainsKey("utterance"))
                return "utterance";

            if (data.ContainsKey("response"))
                return "response";

            return callName;
        }
    }
}
