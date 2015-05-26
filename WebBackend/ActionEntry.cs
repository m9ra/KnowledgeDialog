using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;


namespace WebBackend
{
    class ActionEntry
    {
        public readonly DateTime Time;

        public readonly string Text;

        public readonly string Type;

        public readonly string UserId;

        internal bool HasUserId()
        {
            return UserId != null;
        }

        internal ActionEntry(Dictionary<string, object> data)
        {
            if (data.ContainsKey(CallStorage.TimeEntry))
                Time = (DateTime)data[CallStorage.TimeEntry];

            if (data.ContainsKey("user_id"))
                UserId = "user_" + (data["user_id"] as string) + ".json";

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

            if (Type == "T_advice")
            {
                var text = "<b>Question</b>:" + data["question"] + "<br>";
                text += "<b>Context</b>:" + data["context"] + "<br>";
                text += "<b>Answer</b>:" + data["correctAnswerNode"] + "<br><br>";

                Text = text;
            }

            if (Type == "T_equivalence")
            {
                var text = "<b>PatternQuestion</b>:" + data["patternQuestion"] + "<br>";
                text += "<b>QueriedQuestion</b>:" + data["queriedQuestion"] + "<br>";
                text += "<b>IsEquivalent</b>:" + data["isEquivalent"] + "<br><br>";

                Text = text;
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
