using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;

using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

namespace WebBackend
{
    class Program
    {
        public static string RootPath { get; private set; }

        public static string StoragesPath { get { return "data/storages"; } }

        public static string UserStoragesPath { get { return StoragesPath + "/users"; } }

        public static bool UseWikidata { get; private set; }

        public static ComposedGraph Graph { get; private set; }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Expects path to root folder of web and wikidata DB path");
                return;
            }

            var writer = new ExperimentCodeWriter(ExperimentData.ExperimentId + ".csv");
            for (var i = 0; i < 15; ++i)
            {
                writer.Write(ExperimentData.ExperimentId, i);
            }
            writer.Close();


            var wwwPath = args[0];
            if (args.Length > 1)
            {
                UseWikidata = true;
                var loader = new KnowledgeDialog.Database.TripletLoader.Loader(args[1]);
                Graph = new ComposedGraph(loader.DataLayer);

                WikidataHelper.PreprocessData(loader, Graph);

                //DB TESTING - debug only
                var node1 = Graph.GetNode("USA");
                var node2 = Graph.GetNode("Barack Obama");
                var paths = Graph.GetPaths(node1, node2, 10, 1000).Take(5).ToArray();
                paths = paths;
            }
            else
            {
                Graph = new ComposedGraph(new KnowledgeDialog.Database.FlatPresidentLayer());
            }

            if (!Directory.Exists(wwwPath))
                throw new NotSupportedException("Given path doesn't exists. " + Path.GetFullPath(wwwPath));

            TaskFactory.Init();
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
