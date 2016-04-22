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
    class QuestionDialogDatasetReader
    {
        internal readonly IEnumerable<QuestionDialog> Dialogs;

        internal QuestionDialogDatasetReader(string file)
        {
            var dialogs = new List<QuestionDialog>();
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var dialogJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(line);
                    var label = dialogJson["label"] as JObject;
                    var answerMid = label.GetValue("answer_mid").Value<string>();
                    var question = dialogJson["question"] as string;
                    var annotation = label.GetValue("annotation").Value<string>();
                    var answerTurns = dialogJson["answer_turns"] as JArray;
                    var dialog = new QuestionDialog(answerMid, annotation, question, parseTurns(answerTurns));
                    dialogs.Add(dialog);
                }
            }

            Dialogs = dialogs.ToArray();
        }

        private IEnumerable<DialogTurn> parseTurns(JArray turnArray)
        {
            var result = new List<DialogTurn>();
            foreach (var turnJson in turnArray)
            {
                var output = turnJson.Value<JObject>("output");
                var input = turnJson.Value<JObject>("input");
                var chat = input.Value<string>("chat");
                var transcript = output.Value<string>("transcript");
                var turnIndex = turnJson.Value<int>("turn_index");
                var turn = new DialogTurn(turnIndex, transcript, chat);

                result.Add(turn);
            }

            return result;
        }
    }
}
