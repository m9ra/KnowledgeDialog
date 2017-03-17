using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Collections.Specialized;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebBackend.DataSources
{
    class OmegleManager
    {
        private readonly string _frontendServer = "http://front{0}.omegle.com/";

        private readonly CookieAwareWebClient _client = new CookieAwareWebClient(2 * 30 * 1000);

        private string _clientId;

        internal bool IsOnline { get; private set; }

        internal bool IsLoggingEnabled = true;

        internal OmegleManager(int frontendNumber = 1)
        {
            _frontendServer = string.Format(_frontendServer, frontendNumber);
        }

        internal string GetQuestionAnswer(string question)
        {
            sendUtterance("Hi, I'm doing survey. Help needed!");
            sendUtterance(question);

            var replys = new List<string>();
            while (IsOnline)
            {
                var reply = waitForReply();
                if (reply != null)
                    replys.Add(reply);
            }

            return replys.OrderByDescending(r => r.Length).FirstOrDefault();
        }

        internal string[] ObserveQuestion(string question, int maxTurnLimit = int.MaxValue)
        {
            IsLoggingEnabled = false;

            ensureOnline(question);
            Console.WriteLine("Connected: " + question);
            var utterances = new List<string>();
            while (IsOnline)
            {
                var evt = readEvent();
                var spy1 = readValueSanitized(evt, "spyMessage", "Stranger 1");
                var spy2 = readValueSanitized(evt, "spyMessage", "Stranger 2");

                if (spy1 != null)
                {
                    Console.WriteLine("\tU1: " + spy1);
                    utterances.Add(spy1);
                }

                if (spy2 != null)
                {
                    Console.WriteLine("\tU2: " + spy2);
                    utterances.Add(spy2);
                }

                if (spy1 != null || spy2 != null)
                    --maxTurnLimit;

                if (maxTurnLimit < 0)
                {
                    Console.WriteLine("\tmaxturn limit");
                    break;
                }
            }

            Console.WriteLine("\tdisconnected\n");

            return utterances.Where(u => u.Trim() != "").ToArray();
        }

        private void runConsoleInteraction()
        {
            while (IsOnline)
            {
                var reply = waitForReply();
                if (reply != null)
                {
                    Console.Write("Input: ");
                    var interaction = Console.ReadLine();
                    if (IsOnline)
                        sendUtterance(interaction);
                }
            }
        }

        private string waitForReply()
        {
            string reply = null;
            while (IsOnline && reply == null)
            {
                var evt = readEvent();
                reply = readValue(evt, "gotMessage");
            }

            return reply;
        }

        private void sendUtterance(string utterance)
        {
            ensureOnline();

            var response = _client.SendValues(_frontendServer + "/send", new NameValueCollection
                {
                    {"id",  _clientId },
                    {"msg", utterance }
                });

            log("Sent '{0}' with result {1}", utterance, response);

        }

        private void ensureOnline(string question = null)
        {
            if (IsOnline)
                return;

            string url;
            if (question != null)
            {
                var encodedQuestion = Uri.EscapeDataString(question);
                url = _frontendServer + "/start?rcs=1&firstevents=1&spid=&ask=" + encodedQuestion + "&cansavequestion=0";
            }
            else
            {
                url = _frontendServer + "/start?rcs=1&firstevents=1&spid=&randid=TVM4JGDQ&lang=en";
            }

            var data = _client.SendValues(url, new NameValueCollection());

            log("Connection: {0}", data);

            var obj = JsonConvert.DeserializeObject(data) as JObject;
            _clientId = obj["clientID"].ToString();
            IsOnline = true;

            log("Online with {0}", _clientId);
        }

        private JArray readEvent()
        {
            ensureOnline();
            var eventData = _client.SendValues(_frontendServer + "/events", new NameValueCollection
                {
                  { "id", _clientId }
                });

            log("Event: {0}", eventData);

            if (eventData == "null")
            {
                IsOnline = false;
                log("Disconnected");
            }

            return JsonConvert.DeserializeObject(eventData) as JArray;
        }

        private string readValueSanitized(JArray evt, params string[] path)
        {
            var value = readValue(evt, path);
            if (value != null)
                value = value.Replace('\n', ' ').Trim();

            return value;
        }

        private string readValue(JArray evt, params string[] path)
        {
            if (evt == null)
                return null;

            foreach (JArray dataArray in evt)
            {
                var i = 0;
                for (; i < path.Length; ++i)
                {
                    var value = dataArray[i].ToString();
                    if (value != path[i])
                        return null;
                }

                return dataArray[i].ToString();
            }

            return null;
        }

        private void log(string message, params string[] formatArgs)
        {
            if (!IsLoggingEnabled)
                return;

            Console.WriteLine(message, formatArgs);
        }
    }
}
