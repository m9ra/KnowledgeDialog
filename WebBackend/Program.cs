using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;

namespace WebBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Expects path to root folder of web");
                return;
            }

            var wwwPath = args[0];

            if (!Directory.Exists(wwwPath))
                throw new NotSupportedException("Given path doesn't exists. " + Path.GetFullPath(wwwPath));

            runServer(wwwPath);
            runConsole();
        }


        private static void runServer(string wwwPath)
        {
            var webApp = new DialogWeb(wwwPath);

            ServerEnvironment.AddApplication(webApp);
            var server = ServerEnvironment.Start(4000);
        }

        private static void runConsole()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { OnExit(); };
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();

                switch (keyInfo.KeyChar)
                {
                    case 't':
                        Log.TraceDisabled = !Log.TraceDisabled;
                        break;
                    case 'n':
                        Log.NoticeDisabled = !Log.NoticeDisabled;
                        break;
                }

            } while (keyInfo.Key != ConsoleKey.Escape);

            Environment.Exit(0);
        }

        private static void OnExit()
        {
        }
    }
}
