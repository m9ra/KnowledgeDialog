using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;

using KnowledgeDialog.Knowledge;

namespace WebBackend
{
    class Program
    {
        public static string RootPath { get; private set; }

        public static bool UseWikidata { get; private set; }

        public static ComposedGraph Graph { get; private set; }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Expects path to root folder of web and wikidata DB path");
                return;
            }

            var wwwPath = args[0];
            if (args.Length > 1)
            {
                UseWikidata = true;
                var loader = new KnowledgeDialog.Database.TripletLoader.Loader(args[1]);
                Graph = new ComposedGraph(loader.DataLayer);
            }
            else
            {
                Graph = new ComposedGraph(new KnowledgeDialog.Database.FlatPresidentLayer());
            }

            if (!Directory.Exists(wwwPath))
                throw new NotSupportedException("Given path doesn't exists. " + Path.GetFullPath(wwwPath));

            RootPath = wwwPath;
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
