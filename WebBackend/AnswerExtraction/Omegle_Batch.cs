using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBackend.DataSources;

using System.Threading;
using System.Text.RegularExpressions;

using System.IO;

namespace WebBackend.AnswerExtraction
{
    static class Omegle_Batch
    {
        public static void CollectQuestionData()
        {
            for (var i = 0; i < 100; ++i)
            {
                try
                {
                    var manager = new OmegleManager();
                    var answer = manager.GetQuestionAnswer("Who was the first man on Moon?");
                    if (answer != null)
                    {
                        using (var stream = File.AppendText("omegle.log"))
                            stream.WriteLine(answer);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static void ObserveDialog()
        {
            while (true)
            {
                try
                {
                    var manager = new OmegleManager();
                    manager.ObserveQuestion("Capital of Canada?", 4);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static void ObserveDialogCollection()
        {
            var questionPool = new string[]
            {
                "A meaning between 'nasty' and 'ugly'.",
                "A meaning between 'pretty' and 'beautiful'.",
                "A halfway between 'nasty' and 'ugly'.",
                "A halfway between 'pretty' and 'beautiful'.",
                "A halfway between a 'house' and a 'shed'?",
                "A halfway between 'walking' and 'running'?",
                "Are 'pretty' and 'beautiful' totally same, or is there something in between?",
                "Is there anything between a 'house' and a 'shed'?",
                "Is Trump ugly or nasty or something in between?",
                "What can be ugly but not nasty?",
                "If a girl is neither walking nor running but something in between, how would you call it?",
                "If a construction is something between a house and a shed, how would you call it?"
            };


            if (!Directory.Exists(Configuration.OmegleExperimentsRootPath))
                Directory.CreateDirectory(Configuration.OmegleExperimentsRootPath);

            foreach (var question in questionPool)
            {
                writeQuestionLog(question, null);
            }

            var th = new Thread(() =>
              {
                  var rnd = new Random();
                  while (true)
                  {
                      try
                      {
                          var questionIndex = rnd.Next(questionPool.Length);
                          var question = questionPool[questionIndex];

                          var manager = new OmegleManager(5);
                          var utterances = manager.ObserveQuestion(question, 6);
                          writeQuestionLog(question, utterances);
                      }
                      catch (Exception ex)
                      {
                          Console.WriteLine(ex);
                          Thread.Sleep(5000);
                      }
                  }
              });

            th.Start();
        }

        private static void writeQuestionLog(string question, IEnumerable<string> utterances)
        {
            var rgx = new Regex("[^a-zA-Z0-9 -]");
            var logFile = Path.Combine(Configuration.OmegleExperimentsRootPath, rgx.Replace(question, "") + ".omegle_log");

            if (!File.Exists(logFile))
                File.AppendAllLines(logFile, new[] { question });

            if (utterances == null || !utterances.Any())
                //there is nothing to save
                return;

            File.AppendAllLines(logFile, utterances);
            File.AppendAllLines(logFile, new[] { "\n" });
        }
    }
}
