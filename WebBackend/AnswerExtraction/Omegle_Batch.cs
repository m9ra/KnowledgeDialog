using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBackend.DataSources;

using System.Threading;

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
            var _L_write = new object();
            var question = "Which town was Tony Sucipto born?";
            var logFile = "which_town_was_tony_sucipto_born.txt";

            if (!File.Exists(logFile))
                File.AppendAllLines(logFile, new[] { question });

            for (var i = 11; i <= 11; ++i)
            {
                Thread.Sleep(1000);
                var th = new Thread(() =>
                {
                    //warmup - if connection fails, thread ends
                    try
                    {
                        //(new OmegleManager(i)).ObserveQuestion(question, 4);
                    }
                    catch (Exception ex)
                    {
                        //cannot connect to server
                        Console.WriteLine(ex);
                        return;
                    }

                    while (true)
                    {
                        try
                        {
                            var manager = new OmegleManager(i);
                            var utterances = manager.ObserveQuestion(question, 6);
                            if (utterances.Length == 0)
                                continue;

                            lock (_L_write)
                            {
                                File.AppendAllLines(logFile, utterances);
                                File.AppendAllLines(logFile, new[] { "\n" });
                            }
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
        }
    }
}
