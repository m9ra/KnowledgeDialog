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
                "_2nd_strategy|What are synonyms of nasty and ugly?",
                "_2nd_strategy|What are synonyms of pretty and beautiful?",
                "_2nd_strategy|What are synonyms of house and a shed?",
                "_2nd_strategy|What are synonyms of walking and running?",
                "_synonyms|What are synonyms of ugly?",
                "_synonyms|What are synonyms of nasty?",
                "_what_is_stronger|What is stronger, ugly or nasty?",
                "_what_is_stronger|What is stronger, ugly or gross?",
                "_what_is_stronger|What is stronger, gross or nasty?",
                "_natural_order|Are these in natural order: a shed -- a house -- a garage?",
                "_natural_order|Are these in natural order: a house -- a shed -- a garage?",
                "_natural_order|Are these in natural order: a house -- a garage -- a shed?",
            };

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

                          var manager = new OmegleManager(7);
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

        private static void writeQuestionLog(string questionDescriptor, IEnumerable<string> utterances)
        {
            var rgx = new Regex("[^a-zA-Z0-9 -]");
            var questionParts = questionDescriptor.Split(new[] { '|' }, 2);
            var experimentSuffix = questionParts[0];
            var question = questionParts[1];

            var experimentRoot = Configuration.OmegleExperimentsRootPath + experimentSuffix;
            if (!Directory.Exists(experimentRoot))
                Directory.CreateDirectory(experimentRoot);

            var logFile = Path.Combine(experimentRoot, rgx.Replace(question, "") + ".omegle_log");
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
